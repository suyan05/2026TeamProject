// 아이템 데이터 + ID 자동 생성 기능
using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Item/ItemData")]
public class ItemData : ScriptableObject
{
    public int itemID;              // 고유 ID
    public string itemName;
    public GameObject worldPrefab;
    public Sprite icon;

    private void OnValidate()
    {
        // ID가 0이면 자동 생성
        if (itemID == 0)
            itemID = GetInstanceID();
    }
}