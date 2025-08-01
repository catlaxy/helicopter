using UnityEngine;

/// <summary>
/// 월드 공간에 배치된 오브젝트가 참조할 ItemData를 보관하는 컴포넌트입니다.
/// 주로 AltarSlot 또는 ItemClickPickup 등에서 사용된 뒤,
/// 다시 InventoryManager로 회수할 때 해당 ItemData를 읽어오기 위해 붙입니다.
/// </summary>
public class ItemDataHolder : MonoBehaviour
{
    [Tooltip("획득·사용·결합 등에 사용될 ItemData ScriptableObject")]
    public ItemData data;
}
