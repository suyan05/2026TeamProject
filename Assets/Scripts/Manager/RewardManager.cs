using System.Collections.Generic;
using UnityEngine;
using System.Linq; 

public class RewardManager : MonoBehaviour
{
    // 어디서든 접근 가능하게 싱글톤 설정
    public static RewardManager Instance { get; private set; }

    [Header("아이템 데이터베이스")]
    public List<ItemData> itemDatabase; // 유니티 인스펙터에서 아이템 리스트를 넣으면 됨

    [Header("보상 UI 설정")]
    public GameObject rewardPanel;    // 3택 1 카드가 담긴 부모 패널
    public RewardCard[] cards;        // 연결된 카드 스크립트 3개 

    private void Awake()
    {
        // 싱글톤 초기화
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 시작할 때 보상창은 꺼둡니다.
        if (rewardPanel != null) rewardPanel.SetActive(false);
    }

    // 모든 적 처치 시 보상창이 뜸
    public void ShowRewardSelection()
    {
        if (itemDatabase == null || itemDatabase.Count < 3)
        {
            Debug.LogError("아이템 데이터베이스에 아이템이 최소 3개 이상 필요합니다!");
            return;
        }

        List<ItemData> selectedItems = new List<ItemData>();

        // 중복 없는 3개 아이템을 확률적으로 추출
        while (selectedItems.Count < 3)
        {
            ItemData candidate = GetWeightedRandomItem();
            if (!selectedItems.Contains(candidate))
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
                cards[i].Setup(selectedItems[i]);
            }
        }

        
        Time.timeScale = 0f;
        Debug.Log("보상 선택창 활성화 (게임 일시정지)");
    }

    //  무기 확률 70%, 일반 아이템 30%  랜덤
    private ItemData GetWeightedRandomItem()
    {
        float roll = Random.Range(0f, 100f);

       
        ItemType targetType = (roll < 70f) ? ItemType.Weapon : ItemType.Item;

        
        var filteredList = itemDatabase.FindAll(x => x.itemType == targetType);

        
        if (filteredList.Count == 0)
            return itemDatabase[Random.Range(0, itemDatabase.Count)];

        return filteredList[Random.Range(0, filteredList.Count)];
    }

    // 카드(보상)를 클릭했을 때 호출되는 함수
    public void OnRewardSelected(ItemData selectedItem)
    {
        Debug.Log($"<color=lime>{selectedItem.itemName}</color>을(를) 선택했습니다!");

        

        // 보상창 끄고 게임 재개
        rewardPanel.SetActive(false);
        Time.timeScale = 1f;
    }
}