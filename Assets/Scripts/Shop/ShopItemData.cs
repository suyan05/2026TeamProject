using UnityEngine;

[CreateAssetMenu(fileName = "NewShopItem", menuName = "Shop/ItemData")]
public class ShopItemData : ScriptableObject
{
    public string itemName;
    [TextArea] public string itemDescription;
    public Sprite itemIcon;

    // 기존 ItemData.cs의 ItemRarity를 그대로 사용하여 충돌을 방지합니다.
    public ItemRarity rarity;
    public ItemData actualItemData;

    
    public int GetTierPrice()
    {
        switch (rarity)
        {
            case ItemRarity.Common: return 1; // 1원
            case ItemRarity.Rare: return 2; // 2원
            case ItemRarity.Epic: return 3; // 3원
            default: return 0;
        }
    }
}