using UnityEngine;
using System;

[RequireComponent(typeof(Collider2D))]
public class AltarSlot : MonoBehaviour
{
    [Header("이 제단에 올려야 할 아이템 ID")]
    [Tooltip("ItemData.itemId 값 (예: \"Statue_A\")")]
    public string requiredItemId;

    /// <summary>
    /// 이 제단에 올려진 아이템 데이터를 보관합니다.
    /// </summary>
    [HideInInspector]
    public ItemData placedItem;

    /// <summary>
    /// 아이템이 올려질 때마다 호출됩니다.
    /// </summary>
    public event Action<AltarSlot> OnItemPlaced;

    private void Awake()
    {
        // Trigger 콜라이더여야 합니다.
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    /// <summary>
    /// InventoryItem.OnEndDrag 에서 Detect한 후, 
    /// 이 메서드를 직접 호출해서 아이템을 “올려”주세요.
    /// </summary>
    public void PlaceItem(ItemData data)
    {
        // 어떤 아이템이든 일단 기록만 해두고
        placedItem = data;
        OnItemPlaced?.Invoke(this);
    }
}
