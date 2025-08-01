// Interaction.cs
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class Interaction : MonoBehaviour
{
    [Tooltip("허용할 아이템의 itemId")]
    public string requiredItemId;

    [Header("성공 시 추가 호출 이벤트")]
    public UnityEvent OnInteraction;

    [Header("성공 시 활성화할 오브젝트들")]
    public GameObject[] objectsToEnable;

    [Header("성공 시 비활성화할 오브젝트들")]
    public GameObject[] objectsToDisable;

    /// <summary>
    /// InventoryItem을 받아 처리합니다.
    /// 사용 횟수 차감, 오브젝트 토글, 아이템 소진 시 제거 또는 전환까지 수행.
    /// </summary>
    /// <param name="invItem">드래그된 인벤토리 아이템</param>
    /// <returns>성공 시 true</returns>
    public bool TryInteract(InventoryItem invItem)
    {
        Debug.Log($"[Interaction] TryInteract: 요구={requiredItemId}, 제공={(invItem?.data?.itemId ?? "null")}");

        // 아이템 유효성 및 ID 검사
        if (invItem == null || invItem.data == null)
        {
            Debug.LogWarning("[Interaction] 실패: InventoryItem 또는 data가 null입니다.");
            return false;
        }
        if (invItem.data.itemId != requiredItemId)
        {
            Debug.LogWarning($"[Interaction] 실패: ID 불일치 (제공={invItem.data.itemId}, 요구={requiredItemId})");
            return false;
        }

        // 1) 사용 횟수 차감
        invItem.usesRemaining--;
        Debug.Log($"[Interaction] {invItem.data.itemId} 사용, 남은 횟수={invItem.usesRemaining}");

        // 2) UnityEvent 호출 및 오브젝트 토글
        OnInteraction?.Invoke();
        foreach (var obj in objectsToEnable)
            if (obj != null) obj.SetActive(true);
        foreach (var obj in objectsToDisable)
            if (obj != null) obj.SetActive(false);

        // 3) 소진 시 처리: 대체 또는 제거
        if (invItem.usesRemaining <= 0)
        {
            var invMgr = Object.FindFirstObjectByType<InventoryManager>();
            int slotIndex = invMgr.GetSlotIndexByData(invItem.data);

            Debug.Log($"[Interaction] 소진 감지: slotIndex={slotIndex}, replaceWhenDepleted={invItem.data.replaceWhenDepleted}, replacementItem={(invItem.data.replacementItem?.itemId ?? "null")}");

            if (invItem.data.replaceWhenDepleted && invItem.data.replacementItem != null)
            {
                invMgr.RemoveItem(slotIndex);
                Debug.Log($"[Interaction] 슬롯 {slotIndex}에서 {invItem.data.itemId} 제거, 대체 아이템 {invItem.data.replacementItem.itemId} 추가");
                invMgr.AddItemAtSlot(slotIndex, invItem.data.replacementItem);
            }
            else
            {
                invMgr.RemoveItem(slotIndex);
                Debug.Log($"[Interaction] 슬롯 {slotIndex}에서 {invItem.data.itemId} 제거 (대체 없음)");
            }
        }


        Debug.Log("[Interaction] TryInteract 완료");
        return true;
    }
}
