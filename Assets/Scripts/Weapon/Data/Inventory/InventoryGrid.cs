using UnityEngine;

public class InventoryGrid
{
    public InventorySlot[,] slots;
    public int gridWidth;
    public int gridHeight;

    public InventoryGrid(int width, int height)
    {
        gridWidth = width;
        gridHeight = height;
        slots = new InventorySlot[width, height];
    }

    // 아이템이 여러 칸 차지 가능한지 검사
    public bool CanPlaceItem(ItemData item, int startX, int startY)
    {
        for (int y = 0; y < item.height; y++)
        {
            for (int x = 0; x < item.width; x++)
            {
                int gx = startX + x;
                int gy = startY + y;

                if (gx >= gridWidth || gy >= gridHeight)
                    return false;

                if (slots[gx, gy] != null)
                    return false;
            }
        }
        return true;
    }

    // 아이템을 여러 칸에 배치
    public void PlaceItem(ItemData item, int startX, int startY)
    {
        for (int y = 0; y < item.height; y++)
        {
            for (int x = 0; x < item.width; x++)
            {
                slots[startX + x, startY + y] = new InventorySlot(item);
            }
        }
    }

    // 아이템 제거
    public void RemoveItem(ItemData item)
    {
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (slots[x, y] != null && slots[x, y].item == item)
                    slots[x, y] = null;
            }
        }
    }

    public void DebugPrintGrid()
    {
        string result = "";

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                result += slots[x, y] == null ? "[ ]" : "[X]";
            }
            result += "\n";
        }

        Debug.Log(result);
    }

}
