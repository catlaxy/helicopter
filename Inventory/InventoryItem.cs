// InventoryItem.cs
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 드래그·드롭 가능한 인벤토리 아이템.
/// AltarSlot 또는 Interaction 컴포넌트에 드랍했을 때 처리하며,
/// 빈 UI 슬롯 위에 올리면 해당 슬롯으로 이동합니다.
/// </summary>
[RequireComponent(typeof(CanvasGroup), typeof(Image))]
public class InventoryItem : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public ItemData data;               // 슬롯에 할당된 아이템 데이터
    [HideInInspector] public GameObject sourceWorldObject; // 원본 월드 오브젝트
    [HideInInspector] public int usesRemaining;            // 남은 사용 횟수

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Transform originalParent;
    private Canvas rootCanvas;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        rootCanvas = GetComponentInParent<Canvas>();
    }

    /// <summary>
    /// 인벤토리에 생성될 때 호출: 아이템 데이터와 남은 사용 횟수 초기화
    /// </summary>
    public void Init(ItemData itemData, GameObject worldObj = null)
    {
        data = itemData;
        sourceWorldObject = worldObj;
        usesRemaining = data.maxUses;
        var img = GetComponent<Image>();
        img.sprite = data.icon;
        img.color = Color.white;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        transform.SetParent(rootCanvas.transform, true);
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
        Debug.Log($"[InventoryItem] OnBeginDrag: {data.itemId}");
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / rootCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 드래그 중 비활성화했던 Raycast 복구
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        Debug.Log($"[InventoryItem] OnEndDrag 시작 (item={data.itemId}, usesRemaining={usesRemaining})");

        // 1) UI 슬롯 위에 드롭되었는지 검사
        if (eventData.pointerEnter != null)
        {
            var slot = eventData.pointerEnter.GetComponent<InventorySlot>();
            if (slot != null)
            {
                Debug.Log($"[InventoryItem] 슬롯 드롭: {slot.name}");
                // 슬롯이 비어 있으면 옮기기
                transform.SetParent(slot.transform, false);
                rectTransform.anchoredPosition = Vector2.zero;
                return;
            }
        }

        // 2) 월드 상호작용 검사 (AltarSlot, Interaction)
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
        int mask = LayerMask.GetMask("Altar", "Interaction");
        Collider2D hit = Physics2D.OverlapPoint(worldPos, mask);
        Debug.Log($"[InventoryItem] 월드 충돌 대상: {(hit != null ? hit.name : "없음")}");

        if (hit != null)
        {
            // 2-1) AltarSlot 처리
            var altar = hit.GetComponent<AltarSlot>();
            if (altar != null)
            {
                Debug.Log($"[InventoryItem] AltarSlot 발견: {altar.name} → PlaceItem");
                altar.PlaceItem(data);

                // 인벤토리에서 해당 슬롯 삭제
                var invMgr = Object.FindFirstObjectByType<InventoryManager>();
                int idx = invMgr.GetSlotIndexByData(data);
                invMgr.RemoveItem(idx);
                Debug.Log($"[InventoryItem] 슬롯 {idx} 아이템 제거 (월드 재배치)");

                // 월드 오브젝트 재활용
                if (sourceWorldObject != null)
                {
                    sourceWorldObject.transform.position = altar.transform.position;
                    sourceWorldObject.SetActive(true);
                    sourceWorldObject = null;
                }
                return;
            }

            // 2-2) Interaction 처리
            var interaction = hit.GetComponent<Interaction>();
            if (interaction != null)
            {
                Debug.Log($"[InventoryItem] Interaction 발견: {interaction.name} → TryInteract 호출");
                if (interaction.TryInteract(this))
                {
                    Debug.Log($"[InventoryItem] Interaction 성공, usesRemaining={usesRemaining}");

                    if (usesRemaining > 0)
                    {
                        // 남은 사용 횟수가 있으면 슬롯으로 복귀
                        transform.SetParent(originalParent, false);
                        rectTransform.anchoredPosition = Vector2.zero;
                        Debug.Log($"[InventoryItem] 슬롯 중앙 복귀: {originalParent.name}");
                    }
                    else
                    {
                        // 소진되었으면 UI 삭제
                        Debug.Log($"[InventoryItem] 사용 소진, UI 객체 삭제: {data.itemId}");
                        Destroy(gameObject);
                    }
                    return;
                }
                else
                {
                    Debug.Log("[InventoryItem] Interaction 실패");
                }
            }
        }

        // 3) 그 외 잘못된 드롭: 원래 슬롯으로 복귀
        Debug.Log($"[InventoryItem] 잘못된 드롭, 원래 슬롯({originalParent.name})으로 복귀");
        transform.SetParent(originalParent, false);
        rectTransform.anchoredPosition = Vector2.zero;
    }

}
