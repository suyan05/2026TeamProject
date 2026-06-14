using System.Collections.Generic;
using UnityEngine;

public class RewardManager : MonoBehaviour
{
    public static RewardManager Instance { get; private set; }

    [Header("아이템 데이터베이스 (여기에 넣은 순서대로 고정!)")]
    public List<ItemData> itemDatabase;

    [Header("보상 UI 설정")]
    public GameObject rewardPanel;
    public RewardCard[] cards;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (rewardPanel != null)
            rewardPanel.SetActive(false);
    }

    public void ShowRewardSelection()
    {
        if (itemDatabase == null || itemDatabase.Count < 3)
        {
            Debug.LogError("아이템 데이터베이스에 아이템을 3개 이상 넣어주세요!");
            return;
        }

        rewardPanel.SetActive(true);

        
        for (int i = 0; i < 3; i++)
        {
            if (i < cards.Length)
            {
                cards[i].gameObject.SetActive(true);
                cards[i].Setup(itemDatabase[i]);
            }
        }

        Time.timeScale = 0f; // 게임 일시정지
        Debug.Log("고정 보상 선택창 활성화 (게임 일시정지)");
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