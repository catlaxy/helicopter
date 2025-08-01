using UnityEngine;

/// <summary>
/// 월드 공간에 배치된 아이템을 클릭하여 획득 처리합니다.
/// 원본 오브젝트를 비활성화하고, InventoryManager로 넘깁니다.
/// </summary>
[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
public class ItemClickPickup : MonoBehaviour
{
    [Tooltip("획득 시 인벤토리에 추가할 아이템 데이터")]
    public ItemData itemData;

    [Tooltip("씬에 배치된 InventoryManager 참조 (인스펙터에 드래그)")]
    public InventoryManager inventoryManager;

    /// <summary>
    /// 아이템 클릭 시 호출.
    /// InventoryManager.AddItem(itemData, this.gameObject) 호출 후 비활성화.
    /// </summary>
    private void OnMouseDown()
    {
        inventoryManager.AddItem(itemData, gameObject);
        gameObject.SetActive(false);
    }
}
