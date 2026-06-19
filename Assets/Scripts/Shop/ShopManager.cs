using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("인벤토리 UI 직접 연결 ")]
    public InventoryUI inventoryUI;

    [Header("상점 전체 패널 오브젝트 (껐다 켜기용)")]
    public GameObject shopPanelObject;

    [Header("전체 아이템 데이터베이스 Pool")]
    public List<ShopItemData> allShopItems = new List<ShopItemData>();

    [Header("상점 배치 UI 카드 슬롯 (총 4개)")]
    public ShopCardUI[] shopCards = new ShopCardUI[4];

    [Header("상점 시스템 버튼 및 UI")]
    public Button rerollButton;
    public Text playerMoneyText;

    [Header("툴팁 UI 오브젝트")]
    public GameObject tooltipPanel;
    public Text tooltipNameText;
    public Text tooltipDescText;

  

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        if (rerollButton != null) rerollButton.onClick.AddListener(RefreshShopProducts);
        HideTooltip();
        UpdateMoneyUI();
        RefreshShopProducts();
    }

    public void OpenShop()
    {
        if (shopPanelObject != null)
        {
            shopPanelObject.SetActive(true);
            RefreshShopProducts();
            UpdateMoneyUI(); 
        }
    }

    public void CloseShop()
    {
        if (shopPanelObject != null) { shopPanelObject.SetActive(false); HideTooltip(); }
    }

    public void RefreshShopProducts()
    {
        if (allShopItems.Count < 4) return;
        List<ShopItemData> tempPool = new List<ShopItemData>(allShopItems);
        for (int i = 0; i < shopCards.Length; i++)
        {
            if (shopCards[i] == null) continue;
            int randomIndex = Random.Range(0, tempPool.Count);
            shopCards[i].SetCard(tempPool[randomIndex]);
            tempPool.RemoveAt(randomIndex);
        }
        HideTooltip();
    }

    public void TryPurchaseItem(ShopCardUI clickedCard)
    {
        ShopItemData targetItem = clickedCard.itemData;
        int cost = targetItem.GetTierPrice();

      
        if (CurrencyManager.Instance.HasEnoughGold(cost))
        {
            InventoryUI invUI = inventoryUI;
            if (invUI == null) invUI = FindObjectOfType<InventoryUI>(true);
            if (invUI != null && invUI.inventory == null) invUI.inventory = FindObjectOfType<Inventory>(true);

            Inventory directInv = (invUI != null) ? invUI.inventory : FindObjectOfType<Inventory>(true);

            if (directInv != null)
            {
                
                if (directInv.TryAddItem(targetItem.actualItemData))
                {
                    
                    CurrencyManager.Instance.SpendGold(cost);

                    UpdateMoneyUI();
                    if (invUI != null) invUI.RefreshItems();
                    clickedCard.gameObject.SetActive(false);
                    HideTooltip();
                    Debug.Log($"<{targetItem.itemName}> 구매 성공! 남은 잔액: {CurrencyManager.Instance.Gold}원");
                    return;
                }
            }
            Debug.LogError("씬에 인벤토리(Inventory) 데이터가 없거나 가방이 꽉 찼습니다!");
        }
        else
        {
            Debug.Log("골드가 부족하여 구매할 수 없습니다.");
        }
    }

   
    public void UpdateMoneyUI()
    {
        if (playerMoneyText != null && CurrencyManager.Instance != null)
            playerMoneyText.text = $"보유 재화: {CurrencyManager.Instance.Gold}원";
    }

    public void ShowTooltip(ShopItemData data, Vector3 cardPosition)
    {
        if (tooltipPanel == null) return;
        tooltipPanel.SetActive(true);
        tooltipNameText.text = data.itemName;
        tooltipDescText.text = data.itemDescription;
        tooltipPanel.transform.position = cardPosition + new Vector3(0, 150f, 0);
    }
    public void HideTooltip() { if (tooltipPanel != null) tooltipPanel.SetActive(false); }
}