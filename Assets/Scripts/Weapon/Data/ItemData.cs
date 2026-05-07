using UnityEngine;

public enum ItemType
{
    None,
    Weapon,
    Item
}

public enum WeaponType
{
    None,
    OneHandSword,
    TwoHandSword,
    Bow,
    Dagger,
    Spear,
    Staff
}

public enum ElementType
{
    None,
    Fire,
    Ice,
    Lightning,
    Poison
}

[CreateAssetMenu(fileName = "NewItem", menuName = "Item/ItemData")]
public class ItemData : ScriptableObject
{
    [Header("아이템 고유 ID")]
    [SerializeField] public int itemID;
    public int ItemID => itemID;

    [Header("아이템 이름")]
    public string itemName;

    [Header("아이템 설명")]
    [TextArea] public string description;

    [Header("아이템 타입")]
    public ItemType itemType = ItemType.None;

    [Header("3D 월드에 떨어질 프리팹")]
    public GameObject worldPrefab;

    [Header("플레이어 장착용 프리팹 (손에 들 무기)")]
    public GameObject equipPrefab;

    [Header("UI 아이콘")]
    public Sprite icon;

    [Header("아이템 크기 (인벤토리 슬롯 차지 크기)")]
    public int width = 1;
    public int height = 1;

    // 무기 전용 스탯
    [Header("무기 전용 스탯")]
    public WeaponType weaponType = WeaponType.None;

    public float weaponAttackPower = 0f;
    public float weaponAttackSpeed = 1f;
    public ElementType elementType = ElementType.None;

    // 아이템 전용 스탯 보너스
    [Header("아이템 전용 스탯 보너스")]
    public float bonusMaxHp = 0f;
    public float bonusBaseDamage = 0f;
    public float bonusAttackSpeed = 0f;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (itemID == 0)
            itemID = GetInstanceID();

        if (string.IsNullOrWhiteSpace(itemName))
            Debug.LogWarning($"{name} : 아이템 이름이 비어 있습니다.", this);
        if (icon == null)
            Debug.LogWarning($"{name} : 아이콘이 지정되지 않았습니다.", this);
    }
#endif

    private void OnEnable()
    {
        if (itemID == 0)
            itemID = GetInstanceID();
    }
}
