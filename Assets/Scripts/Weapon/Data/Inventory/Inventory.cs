using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    public int gridWidth = 6;
    public int gridHeight = 6;

    public InventoryGrid grid;
    public List<ItemInstance> items = new List<ItemInstance>();

    void Awake()
    {
        grid = new InventoryGrid(gridWidth, gridHeight);
    }

    public bool TryAddItem(ItemData data)
    {
        ItemInstance instance = new ItemInstance(data);

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (grid.CanPlaceItem(instance, x, y))
                {
                    grid.PlaceItem(instance, x, y);
                    items.Add(instance);
                    PlayerMovement.Instance.RecalculateStats(items);
                    return true;
                }
            }
        }
        return false;
    }

    public void RemoveItem(ItemInstance instance)
    {
        grid.RemoveItem(instance);
        items.Remove(instance);
        PlayerMovement.Instance.RecalculateStats(items);
    }

    public Dictionary<ItemInstance, Vector2Int> GetItemPositions()
    {
        Dictionary<ItemInstance, Vector2Int> dict = new Dictionary<ItemInstance, Vector2Int>();

        foreach (var item in items)
        {
            for (int y = 0; y < grid.gridHeight; y++)
            {
                for (int x = 0; x < grid.gridWidth; x++)
                {
                    var slot = grid.slots[x, y];
                    if (slot != null && slot.item.uniqueID == item.uniqueID)
                    {
                        dict[item] = new Vector2Int(x, y);
                        goto Found;
                    }
                }
            }
        Found:;
        }

        return dict;
    }
    public Vector2Int FindClosestEmptySlot(Vector2 screenPosition, InventoryUI ui)
{
    float bestDist = float.MaxValue;
    Vector2Int bestSlot = new Vector2Int(-1, -1);

    for (int y = 0; y < grid.gridHeight; y++)
    {
        for (int x = 0; x < grid.gridWidth; x++)
        {
            if (grid.slots[x, y] != null) continue; // 이미 차있으면 패스

            // 슬롯 UI의 화면 좌표 가져오기
            RectTransform slotRect = ui.slotUIs[x, y].GetComponent<RectTransform>();
            Vector2 slotScreenPos = RectTransformUtility.WorldToScreenPoint(null, slotRect.position);

            float dist = Vector2.Distance(screenPosition, slotScreenPos);

            if (dist < bestDist)
            {
                bestDist = dist;
                bestSlot = new Vector2Int(x, y);
            }
        }
    }

    return bestSlot;
}
}
