using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    public int gridWidth = 6;
    public int gridHeight = 6;

    public InventoryGrid grid;
    public List<ItemData> items = new List<ItemData>();

    void Awake()
    {
        grid = new InventoryGrid(gridWidth, gridHeight);
    }

    // 아이템 추가
    public bool TryAddItem(ItemData item)
    {
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (grid.CanPlaceItem(item, x, y))
                {
                    grid.PlaceItem(item, x, y);
                    items.Add(item);

                    grid.DebugPrintGrid();

                    return true;
                }
            }
        }
        return false;
    }

    public void RemoveItem(ItemData item)
    {
        grid.RemoveItem(item);
        items.Remove(item);

        grid.DebugPrintGrid();
    }

}
