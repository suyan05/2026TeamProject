using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardCard : MonoBehaviour
{
    [Header("UI 연결")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;
    public Image iconImage;

    private ItemData itemData; // 내가 품고 있는 아이템 정보

    public void Setup(ItemData data)
    {
        itemData = data; 

        if (nameText != null) nameText.text = data.itemName;
        if (descText != null) descText.text = data.description;
        if (iconImage != null) iconImage.sprite = data.icon;
    }

   
    public void OnClickCard()
    {
        if (itemData != null)
        {
           
            RewardManager.Instance.OnRewardSelected(itemData);
        }
        else
        {
            Debug.LogWarning("카드가 제대로 비어있는 상태에서 클릭되었습니다.");
        }
    }
}