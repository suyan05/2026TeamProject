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
        itemData = data;
        nameText.text = data.itemName;
        descText.text = data.description;
        iconImage.sprite = data.icon;
    }

    // 카드(버튼)를 클릭했을 때 실행될 함수
    public void OnClickCard()
    {
        RewardManager.Instance.OnRewardSelected(itemData);
    }
}