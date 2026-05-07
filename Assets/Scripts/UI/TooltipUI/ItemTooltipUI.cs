using UnityEngine;
using UnityEngine.UI;

public class ItemTooltipUI : MonoBehaviour
{
    public Text itemNameText;
    public Text descriptionText;
    public Text statText;

    public void SetData(ItemData data)
    {
        itemNameText.text = data.itemName;
        descriptionText.text = data.description;

        string stats = "";

        if (data.itemType == ItemType.Weapon)
        {
            stats += $"<b>[무기]</b>\n";
            stats += $"타입: {data.weaponType}\n";
            stats += $"공격력: {data.weaponAttackPower}\n";
            stats += $"공격 속도: {data.weaponAttackSpeed}\n";
            stats += $"속성: {data.elementType}\n";
        }
        else if (data.itemType == ItemType.Item)
        {
            stats += $"<b>[아이템 효과]</b>\n";
            if (data.bonusMaxHp != 0) stats += $"최대 체력 +{data.bonusMaxHp}\n";
            if (data.bonusBaseDamage != 0) stats += $"공격력 +{data.bonusBaseDamage}\n";
            if (data.bonusAttackSpeed != 0) stats += $"공격 속도 +{data.bonusAttackSpeed}\n";
        }

        statText.text = stats;
    }
}
