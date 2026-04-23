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
    }
}
