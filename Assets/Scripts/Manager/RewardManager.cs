using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RewardManager : MonoBehaviour
{
    public static RewardManager Instance { get; private set; }

    [Header("아이템 데이터베이스")]
    public List<ItemData> itemDatabase;

    [Header("보상 UI 설정")]
    public GameObject rewardPanel;
    public RewardCard[] cards;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (rewardPanel != null) rewardPanel.SetActive(false);
    }

    public void ShowRewardSelection()
    {
        if (itemDatabase == null || itemDatabase.Count == 0)
        {
            Debug.LogError("아이템 데이터베이스가 완전히 비어있습니다!");
            return;
        }

        int targetCount = Mathf.Min(3, itemDatabase.Count);
        List<ItemData> selectedItems = new List<ItemData>();

        int loopSafety = 0;
        while (selectedItems.Count < targetCount && loopSafety < 100)
        {
            loopSafety++;
            ItemData candidate = GetWeightedRandomItem();

            if (candidate != null && !selectedItems.Contains(candidate))
            {
                selectedItems.Add(candidate);
            }
        }

        rewardPanel.SetActive(true);
        for (int i = 0; i < cards.Length; i++)
        {
            if (i < selectedItems.Count)
            {
                cards[i].gameObject.SetActive(true);
                cards[i].Setup(selectedItems[i]); // 카드에 데이터 주입
            }
            else
            {
                cards[i].gameObject.SetActive(false);
            }
        }

        Time.timeScale = 0f; // 게임 일시정지
        Debug.Log("보상 선택창 활성화 (게임 일시정지)");
    }

    private ItemData GetWeightedRandomItem()
    {
        float roll = Random.Range(0f, 100f);
        ItemType targetType = (roll < 70f) ? ItemType.Weapon : ItemType.Item;

        var filteredList = itemDatabase.FindAll(x => x.itemType == targetType);

        if (filteredList.Count == 0)
            return itemDatabase[Random.Range(0, itemDatabase.Count)];

        return filteredList[Random.Range(0, filteredList.Count)];
    }

    
    public void OnRewardSelected(ItemData selectedItem)
    {
        if (selectedItem != null)
        {
            Debug.Log($"<color=lime>{selectedItem.itemName}</color> 획득 완료! 즉시 게임으로 돌아갑니다.");


        }

        
        if (rewardPanel != null) rewardPanel.SetActive(false);
        Time.timeScale = 1f;
    }
}