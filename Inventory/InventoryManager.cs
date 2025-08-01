// InventoryManager.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 인벤토리 아이템을 슬롯에 추가·제거하고,
/// 아이템 간 조합 기능을 제공합니다.
/// </summary>
public class InventoryManager : MonoBehaviour
{
    [Header("1) 슬롯 부모 Transform 리스트")]
    [Tooltip("각 슬롯(빈 칸)의 Transform을 순서대로 할당하세요.")]
    public List<Transform> slotParents;

    [Header("2) 씬 내 비활성화된 템플릿 오브젝트")]
    [Tooltip("씬에 미리 배치하고 비활성화한 InventoryItem 템플릿 오브젝트")]
    public GameObject itemTemplate;

    // 슬롯별 저장된 ItemData 참조
    private List<ItemData> storedItems;

    private void Awake()
    {
        storedItems = new List<ItemData>(new ItemData[slotParents.Count]);
        if (itemTemplate.activeSelf)
            itemTemplate.SetActive(false);
    }

    /// <summary>
    /// 빈 슬롯을 찾아 InventoryItem 인스턴스를 생성 및 초기화합니다.
    /// </summary>
    public void AddItem(ItemData data, GameObject sourceWorldObject = null)
    {
        for (int i = 0; i < slotParents.Count; i++)
        {
            if (slotParents[i].childCount == 0)
            {
                CreateItemInSlot(i, data, sourceWorldObject);
                return;
            }
        }
        Debug.LogWarning("인벤토리 슬롯이 가득 찼습니다.");
    }

    /// <summary>
    /// 지정한 슬롯에 아이템을 생성 및 초기화합니다.
    /// </summary>
    public void AddItemAtSlot(int slotIndex, ItemData data, GameObject sourceWorldObject = null)
    {
        Debug.Log($"[InventoryManager] AddItemAtSlot 호출: slotIndex={slotIndex}, itemId={data.itemId}");

        if (slotIndex < 0 || slotIndex >= slotParents.Count)
        {
            Debug.LogError($"[InventoryManager] AddItemAtSlot: 유효하지 않은 슬롯 인덱스 {slotIndex}");
            return;
        }

        // 슬롯이 비어 있지 않으면 경고 후 빈 슬롯에 추가
        if (slotParents[slotIndex].childCount != 0)
        {
            Debug.LogWarning($"[InventoryManager] AddItemAtSlot: 슬롯 {slotIndex}가 비어있지 않습니다. AddItem 으로 대체");
            AddItem(data, sourceWorldObject);
            return;
        }

        Debug.Log($"[InventoryManager] 슬롯 {slotIndex}에 아이템 생성 시작");
        CreateItemInSlot(slotIndex, data, sourceWorldObject);
        Debug.Log($"[InventoryManager] 슬롯 {slotIndex}에 아이템 {data.itemId} 생성 완료");
    }


    /// <summary>
    /// 실제 아이템 프리팹을 슬롯에 생성하고 초기화하는 내부 헬퍼.
    /// </summary>
    private void CreateItemInSlot(int index, ItemData data, GameObject sourceWorldObject)
    {
        Transform parent = slotParents[index];
        var go = Instantiate(itemTemplate, parent, false);
        go.SetActive(true);

        var invItem = go.GetComponent<InventoryItem>();
        invItem.Init(data, sourceWorldObject);

        storedItems[index] = data;

        if (sourceWorldObject != null)
            sourceWorldObject.SetActive(false);
    }

    /// <summary>
    /// 지정된 슬롯 인덱스를 비우고, 해당 슬롯 자식과 데이터를 모두 제거합니다.
    /// </summary>
    public void RemoveItem(int index)
    {
        if (index < 0 || index >= slotParents.Count) return;
        foreach (Transform child in slotParents[index])
            Destroy(child.gameObject);
        storedItems[index] = null;
    }

    public ItemData GetSlotData(int index)
    {
        if (index < 0 || index >= storedItems.Count) return null;
        return storedItems[index];
    }

    public int GetSlotIndexByData(ItemData data)
    {
        return storedItems.IndexOf(data);
    }

    private ItemData GetCombinationResult(ItemData a, ItemData b)
    {
        foreach (var combo in a.combinations)
            if (combo.otherItemId == b.itemId)
                return combo.resultItem;
        foreach (var combo in b.combinations)
            if (combo.otherItemId == a.itemId)
                return combo.resultItem;
        return null;
    }

    public bool TryCombine(ItemData a, ItemData b)
    {
        var result = GetCombinationResult(a, b);
        if (result == null) return false;

        int idxA = GetSlotIndexByData(a);
        int idxB = GetSlotIndexByData(b);
        if (idxA < 0 || idxB < 0) return false;

        RemoveItem(idxA);
        idxB = GetSlotIndexByData(b);
        RemoveItem(idxB);

        AddItem(result);
        Debug.Log($"[InventoryManager] {a.itemId} + {b.itemId} 조합 성공 → {result.itemId} 추가");
        return true;
    }
}
