using UnityEngine;

public class InventoryGrid : MonoBehaviour
{
    [Header("백팩 그리드 크기 (가로 × 세로)")]
    public int gridWidth = 10;
    public int gridHeight = 6;

    // 각 칸에 어떤 아이템이 있는지 저장하는 2D 배열
    private ItemData[,] grid;

    void Awake()
    {
        grid = new ItemData[gridWidth, gridHeight];
    }

    // 특정 위치에 아이템을 놓을 수 있는지 검사
    public bool CanPlaceItem(ItemData item, int x, int y)
    {
        for (int i = 0; i < item.width; i++)
        {
            for (int j = 0; j < item.height; j++)
            {
                int gx = x + i;
                int gy = y + j;

                // 그리드 범위 밖이면 불가능
                if (gx < 0 || gy < 0 || gx >= gridWidth || gy >= gridHeight)
                    return false;

                // 이미 다른 아이템이 차지하고 있으면 불가능
                if (grid[gx, gy] != null)
                    return false;
            }
        }
        return true;
    }

    // 아이템 배치
    public void PlaceItem(ItemData item, int x, int y)
    {
        for (int i = 0; i < item.width; i++)
            for (int j = 0; j < item.height; j++)
                grid[x + i, y + j] = item;
    }

    // 아이템 제거
    public void RemoveItem(ItemData item)
    {
        for (int x = 0; x < gridWidth; x++)
            for (int y = 0; y < gridHeight; y++)
                if (grid[x, y] == item)
                    grid[x, y] = null;
    }
}
