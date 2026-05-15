using UnityEngine;
using UnityEngine.UI;
using TMPro; // 텍스트에 TextMeshPro를 사용하신다면 추가

public class RewardCard : MonoBehaviour
{
    [Header("UI 연결")]
    public TextMeshProUGUI nameText;    // 아이템 이름 텍스트
    public TextMeshProUGUI descText;    // 아이템 설명 텍스트
    public Image iconImage;             // 아이템 아이콘 이미지

    private ItemData itemData;

    // 보상 정보를 카드 UI에 세팅하는 함수
    public void Setup(ItemData data)
    {
        // 이 if문을 추가해서 "글자 상자가 있을 때만" 이름을 쓰게 만듭니다.
        if (nameText != null)
        {
            nameText.text = data.itemName;
        }

        // 설명이나 아이콘도 마찬가지로 if 처리를 해두면 좋습니다.
        if (descText != null) descText.text = data.description;
        if (iconImage != null) iconImage.sprite = data.icon;
    }

    // 카드(버튼)를 클릭했을 때 실행될 함수
    public void OnClickCard()
    {
        RewardManager.Instance.OnRewardSelected(itemData);
    }
}