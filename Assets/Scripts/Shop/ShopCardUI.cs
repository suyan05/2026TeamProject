using UnityEngine;
using UnityEngine.UI;

public class ShopCardUI : MonoBehaviour
{
    public Image itemIconImage;
    public Text itemNameText;
    public Text itemPriceText;
    public Text itemRarityText;
    public Button buyButton;

    [HideInInspector] public ShopItemData itemData;

    public void SetCard(ShopItemData newData)
    {
        itemData = newData;

        if (itemData == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);
        itemIconImage.sprite = itemData.itemIcon;
        itemNameText.text = itemData.itemName;
        itemPriceText.text = $"{itemData.GetTierPrice()}원";
        itemRarityText.text = $"[{itemData.rarity.ToString()}]";

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnBuyClicked);

        // 마우스 오버 감지용 툴팁 트리거 연결
        ShopTooltipTrigger trigger = GetComponent<ShopTooltipTrigger>() ?? gameObject.AddComponent<ShopTooltipTrigger>();
        trigger.Setup(this);
    }

    private void OnBuyClicked()
    {
        ShopManager.Instance.TryPurchaseItem(this);
    }
}