// ItemData.cs
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Inventory/ItemData")]
public class ItemData : ScriptableObject
{
    [Header("기본 정보")]
    [Tooltip("고유 ID (예: \"Driver\", \"RubberGlove\")")]
    public string itemId;

    [Tooltip("인벤토리에 표시할 아이콘")]
    public Sprite icon;

    [Tooltip("아이템 표시 이름")]
    public string itemName;

    [TextArea, Tooltip("아이템 설명")]
    public string description;

    [Header("재사용 설정")]
    [Tooltip("아이템이 사용될 수 있는 최대 횟수. 1이면 한 번 사용 후 소모")]
    public int maxUses = 1;

    [Tooltip("사용 횟수 소진 시 다른 아이템으로 전환할지 여부")]
    public bool replaceWhenDepleted = false;

    [Tooltip("소진 후 인벤토리에 추가할 아이템 데이터")]
    public ItemData replacementItem;

    [Header("조합 설정")]
    [Tooltip("이 아이템과 조합할 다른 아이템의 ID와 결과")]
    public List<Combination> combinations;

    [System.Serializable]
    public struct Combination
    {
        [Tooltip("조합 상대 아이템의 ID")]
        public string otherItemId;
        [Tooltip("조합 성공 시 생성할 아이템 데이터")]
        public ItemData resultItem;
    }
}
