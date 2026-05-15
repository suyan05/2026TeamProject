using UnityEngine;


public enum ItemType { None, Weapon, Item }
public enum WeaponType { None, OneHandSword, TwoHandSword, Bow, Dagger, Spear, Staff }
public enum ElementType { None, Fire, Ice, Lightning, Poison }

// 등급 시스템 추가
public enum ItemRarity { Common, Rare, Epic }

[CreateAssetMenu(fileName = "NewItem", menuName = "Item/ItemData")]
public class ItemData : ScriptableObject
{
    [Header("아이템 등급 (팀장님 요청: 확률용)")]
    public ItemRarity rarity = ItemRarity.Common;

    [Header("아이템 고유 ID")]
    [SerializeField] private int itemID;
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

    [Header("아이템 크기")]
    public int width = 1;
    public int height = 1;

    [Header("무기 전용 스탯")]
    public WeaponType weaponType = WeaponType.None;
    public float weaponAttackPower = 0f;
    public float weaponAttackSpeed = 1f;
    public ElementType elementType = ElementType.None;

    [Header("아이템 전용 스탯 보너스")]
    public float bonusMaxHp = 0f;
    public float bonusBaseDamage = 0f;
    public float bonusAttackSpeed = 0f;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (itemID == 0) itemID = GetInstanceID();
    }
#endif

    private void OnEnable()
    {
        if (itemID == 0) itemID = GetInstanceID();
    }
}
