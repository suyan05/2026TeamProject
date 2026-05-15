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
        // [수정] 데이터베이스가 비어있으면 즉시 리턴하여 에디터 멈춤 방지
        if (itemDatabase == null || itemDatabase.Count == 0)
        {
            Debug.LogError("아이템 데이터베이스가 완전히 비어있습니다!");
            return;
        }

        // [추가] 실제 뽑을 개수를 결정 (아이템이 3개 미만이면 가진 만큼만 뽑음)
        int targetCount = Mathf.Min(3, itemDatabase.Count);
        List<ItemData> selectedItems = new List<ItemData>();

        // [수정] 무한 루프 방지를 위한 카운트 제한
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

        // UI 표시 및 카드 데이터 세팅
        rewardPanel.SetActive(true);
        for (int i = 0; i < cards.Length; i++)
        {
            if (i < selectedItems.Count)
            {
                cards[i].gameObject.SetActive(true); // 카드 활성화
                cards[i].Setup(selectedItems[i]);
            }
            else
            {
                cards[i].gameObject.SetActive(false); // 남는 카드는 끄기
            }
        }

        Time.timeScale = 0f;
        Debug.Log("보상 선택창 활성화 (게임 일시정지)");
    }

    private ItemData GetWeightedRandomItem()
    {
        float roll = Random.Range(0f, 100f);
        ItemType targetType = (roll < 70f) ? ItemType.Weapon : ItemType.Item;

        var filteredList = itemDatabase.FindAll(x => x.itemType == targetType);

        // 해당 타입 아이템이 없으면 전체에서 랜덤 반환
        if (filteredList.Count == 0)
            return itemDatabase[Random.Range(0, itemDatabase.Count)];

        return filteredList[Random.Range(0, filteredList.Count)];
    }

    public void OnRewardSelected(ItemData selectedItem)
    {
        Debug.Log($"<color=lime>{selectedItem.itemName}</color>을(를) 선택했습니다!");

        rewardPanel.SetActive(false);
        Time.timeScale = 1f;
    }
}