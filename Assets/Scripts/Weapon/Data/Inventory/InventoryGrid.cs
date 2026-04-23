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

    public bool CanPlaceItem(ItemInstance item, int startX, int startY)
    {
        for (int y = 0; y < item.data.height; y++)
        {
            for (int x = 0; x < item.data.width; x++)
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

    public void PlaceItem(ItemInstance item, int startX, int startY)
    {
        for (int y = 0; y < item.data.height; y++)
        {
            for (int x = 0; x < item.data.width; x++)
            {
                slots[startX + x, startY + y] = new InventorySlot(item);
            }
        }
    }

    public void RemoveItem(ItemInstance item)
    {
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (slots[x, y] != null && slots[x, y].item.uniqueID == item.uniqueID)
                    slots[x, y] = null;
            }
        }
    }
}
