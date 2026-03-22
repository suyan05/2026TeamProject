// ID로 ItemData를 찾기 위한 데이터베이스
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Item/Database")]
public class ItemDatabase : ScriptableObject
{
    public List<ItemData> items;

    public ItemData GetItemByID(int id)
    {
        return items.Find(i => i.itemID == id);
    }
}