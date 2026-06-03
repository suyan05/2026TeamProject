using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

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

    private int playerGold = 10; // 테스트용 임시 재화

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (rerollButton != null)
            rerollButton.onClick.AddListener(RefreshShopProducts);

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
        }
    }

   
    public void CloseShop()
    {
        if (shopPanelObject != null)
        {
            shopPanelObject.SetActive(false); 
            HideTooltip();                   
        }
    }

    
    public void RefreshShopProducts()
    {
        if (allShopItems.Count < 4)
        {
            Debug.LogError("상점 아이템 풀에 최소 4개 이상의 아이템 데이터가 필요합니다!");
            return;
        }

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

        if (playerGold >= cost)
        {
            // 비활성화된(꺼져있는) 인벤토리 UI까지 모조리 추적하도록 (true) 옵션을 추가했습니다.
            InventoryUI invUI = FindObjectOfType<InventoryUI>(true);

            if (invUI != null && invUI.inventory != null)
            {
                // 진짜 가방 데이터(inventory)에 아이템 추가 시도
                if (invUI.inventory.TryAddItem(targetItem.actualItemData))
                {
                    playerGold -= cost;
                    UpdateMoneyUI();

                    // 아이템 획득 성공 후 인벤토리 UI 실시간 새로고침
                    invUI.RefreshItems();

                    // 구매 완료된 카드 슬롯 화면에서 숨김 및 툴팁 종료
                    clickedCard.gameObject.SetActive(false);
                    HideTooltip();
                    Debug.Log($"<{targetItem.itemName}> 구매 성공! 잔액: {playerGold}원");
                    return;
                }
                else
                {
                    Debug.LogWarning("인벤토리 가방에 빈 공간이 부족합니다!");
                    return;
                }
            }

            Debug.LogError("씬에서 인벤토리 시스템(InventoryUI)을 찾을 수 없습니다! 하이어라키 창을 확인해 주세요.");
        }
        else
        {
            Debug.LogWarning("보유한 재화(골드)가 부족합니다.");
        }
    }

    public void AddGold(int amount)
    {
        playerGold += amount;
        UpdateMoneyUI();
    }

    private void UpdateMoneyUI()
    {
        if (playerMoneyText != null)
            playerMoneyText.text = $"보유 재화: {playerGold}원";
    }

    public void ShowTooltip(ShopItemData data, Vector3 cardPosition)
    {
        if (tooltipPanel == null) return;

        tooltipPanel.SetActive(true);
        tooltipNameText.text = data.itemName;
        tooltipDescText.text = data.itemDescription;

        // 카드 위치 기준 살짝 위쪽에 툴팁 배치
        tooltipPanel.transform.position = cardPosition + new Vector3(0, 150f, 0);
    }

    public void HideTooltip()
    {
        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);
    }
}