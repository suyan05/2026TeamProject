using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Item/Database")]
public class ItemDatabase : ScriptableObject
{
    [Header("등록된 모든 아이템 리스트")]
    [SerializeField]
    private List<ItemData> items = new List<ItemData>();
    public IReadOnlyList<ItemData> Items => items;

    private Dictionary<int, ItemData> itemDict;

    private void OnEnable()
    {
        BuildDictionary();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        BuildDictionary();
    }
#endif

    private void BuildDictionary()
    {
        itemDict = new Dictionary<int, ItemData>();
        HashSet<int> idSet = new HashSet<int>();

        foreach (var item in items)
        {
            if (item == null) continue;
            int id = item.ItemID;
            if (id == 0)
            {
                Debug.LogWarning($"{item.name} : itemID가 0입니다. ItemData를 확인하세요.", item);
                continue;
            }
            if (!idSet.Add(id))
            {
                Debug.LogWarning($"중복된 itemID({id})가 발견되었습니다: {item.name}", item);
                continue;
            }
            itemDict[id] = item;
        }
    }

    // ID로 아이템 찾기 (빠른 조회)
    public ItemData GetItemByID(int id)
    {
        if (itemDict != null && itemDict.TryGetValue(id, out var item))
            return item;
        return null;
    }
}
