// InventorySlot.cs
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 슬롯에 아이템을 드롭할 때 처리합니다.
/// - 빈 슬롯: 단순 배치
/// - 점유 슬롯: 조합 시도 성공 시 원본 삭제, 실패 시 복귀
/// </summary>
public class InventorySlot : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        var dragged = eventData.pointerDrag?.GetComponent<InventoryItem>();
        if (dragged == null) return;

        // InventoryManager 참조 (싱글톤 아님)
        var invMgr = Object.FindFirstObjectByType<InventoryManager>();
        if (invMgr == null)
        {
            Debug.LogError("InventoryManager를 찾을 수 없습니다.");
            dragged.OnEndDrag(eventData);
            return;
        }

        if (transform.childCount == 0)
        {
            // 빈 슬롯: 단순 배치
            dragged.transform.SetParent(transform, false);
            dragged.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }
        else
        {
            // 점유 슬롯: 조합 시도
            var existing = transform.GetChild(0).GetComponent<InventoryItem>();
            bool combined = invMgr.TryCombine(dragged.data, existing.data);

            if (!combined)
            {
                // 조합 실패: 원위치 복귀
                dragged.OnEndDrag(eventData);
            }
            else
            {
                // 조합 성공: 드래그 중인 원본 삭제 (새로운 결과만 남김)
                Destroy(dragged.gameObject);
            }
        }
    }
}
