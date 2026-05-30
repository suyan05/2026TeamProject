using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

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

    // 아이템 구매 로직 (컴파일러의 KeyCode 우회 수색 기법 적용)
    public void TryPurchaseItem(ShopCardUI clickedCard)
    {
        ShopItemData targetItem = clickedCard.itemData;
        int cost = targetItem.GetTierPrice();

        if (playerGold >= cost)
        {
            
            GameObject playerObj = GameObject.FindWithTag("Player");

            if (playerObj != null)
            {
               
                Component playerScript = playerObj.GetComponent("PlayerMovement");

                if (playerScript != null)
                {
                    
                    var inventoryField = playerScript.GetType().GetField("inventory");
                    var inventoryUIField = playerScript.GetType().GetField("inventoryUI");

                    if (inventoryField != null)
                    {
                        var inventory = inventoryField.GetValue(playerScript);
                        var inventoryUI = inventoryUIField != null ? inventoryUIField.GetValue(playerScript) : null;

                        if (inventory != null)
                        {
                            
                            var tryAddItemMethod = inventory.GetType().GetMethod("TryAddItem");

                            if (tryAddItemMethod != null)
                            {
                                bool success = (bool)tryAddItemMethod.Invoke(inventory, new object[] { targetItem.actualItemData });

                                if (success)
                                {
                                    // 구매 성공 시 재화 차감 및 UI 갱신
                                    playerGold -= cost;
                                    UpdateMoneyUI();

                                   
                                    if (inventoryUI != null)
                                    {
                                        var refreshMethod = inventoryUI.GetType().GetMethod("RefreshItems");
                                        if (refreshMethod != null)
                                        {
                                            refreshMethod.Invoke(inventoryUI, null);
                                        }
                                    }

                                    
                                    clickedCard.gameObject.SetActive(false);
                                    HideTooltip();
                                    Debug.Log($"<{targetItem.itemName}> 구매 성공! 잔액: {playerGold}원");
                                    return;
                                }
                                else
                                {
                                    Debug.LogWarning("인벤토리에 빈 공간이 없습니다!");
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            Debug.LogError("씬에서 플레이어('Player' 태그 설정 확인) 또는 인벤토리 시스템을 찾을 수 없습니다.");
        }
        else
        {
            Debug.LogWarning("보유한 재화가 부족합니다.");
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

        // 마우스 오버한 카드 슬롯 살짝 위쪽에 툴팁 배치
        tooltipPanel.transform.position = cardPosition + new Vector3(0, 150f, 0);
    }

    public void HideTooltip()
    {
        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);
    }
}