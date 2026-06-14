using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardCard : MonoBehaviour
{
    [Header("UI 연결")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;

    [Header("아이콘 오브젝트 설정 (칼/활 각각 연결)")]
    public GameObject swordIconObj; 
    public GameObject bowIconObj;   

    private ItemData itemData; 

    public void Setup(ItemData data)
    {
        itemData = data;

       
        if (swordIconObj != null) swordIconObj.SetActive(false);
        if (bowIconObj != null) bowIconObj.SetActive(false);

        if (data == null) return;

       
        if (nameText != null) nameText.text = data.itemName;
        if (descText != null) descText.text = data.description;

      
        if (data.itemType == ItemType.Weapon)
        {
           
            string itemNameStr = data.itemName.ToLower();

            
            if (itemNameStr.Contains("dagger") || itemNameStr.Contains("sword"))
            {
                if (swordIconObj != null) swordIconObj.SetActive(true);
            }
           
            else if (itemNameStr.Contains("arrow") || itemNameStr.Contains("bow"))
            {
                if (bowIconObj != null) bowIconObj.SetActive(true);
            }
        }
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