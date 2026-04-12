using UnityEngine;

public enum ItemType
{
    None,
    Weapon,
    Item
}

[CreateAssetMenu(fileName = "NewItem", menuName = "Item/ItemData")]
public class ItemData : ScriptableObject
{
    [Header("아이템 고유 ID")]
    [SerializeField]
    public int itemID;
    public int ItemID => itemID;

    [Header("아이템 이름")]
    public string itemName;

    [Header("아이템 설명")]
    [TextArea]
    public string description;

    [Header("아이템 타입")]
    public ItemType itemType = ItemType.None;

    [Header("3D 월드에 떨어질 프리팹")]
    public GameObject worldPrefab;

    [Header("UI 아이콘")]
    public Sprite icon;

    [Header("아이템 크기")] //인벤토리 내부 아이템 슬롯에서 차지하는 크기 (1x1, 1x2, 2x2 등)
    public int width = 1;
    public int height = 1;

#if UNITY_EDITOR
    private void OnValidate()
    {
        // ID가 비어있으면 자동 생성 (직접 입력한 값이 0일 때만 자동 할당)
        if (itemID == 0)
            itemID = GetInstanceID();

        // 필수 필드 누락 시 경고
        if (string.IsNullOrWhiteSpace(itemName))
            Debug.LogWarning($"{name} : 아이템 이름이 비어 있습니다.", this);
        if (icon == null)
            Debug.LogWarning($"{name} : 아이콘이 지정되지 않았습니다.", this);
    }
#endif

    private void OnEnable()
    {
        // 런타임에서도 ID가 보장되도록 처리
        if (itemID == 0)
            itemID = GetInstanceID();
    }
}
