using UnityEngine;
using System;
using System.Collections.Generic;

public class InventoryGrid : MonoBehaviour
{
    [Header("백팩 크기 (칸 단위)")]
    public int gridWidth = 10;
    public int gridHeight = 6;

    private ItemData[,] grid;

    // 아이템의 좌표 저장 (UI 재생성 시 필요)
    private Dictionary<ItemData, Vector2Int> itemPositions = new Dictionary<ItemData, Vector2Int>();

    // 그리드 확장 시 UI에게 알려주는 이벤트
    public event Action OnGridExpanded;

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

                // 범위 벗어나면 배치 불가
                if (gx < 0 || gy < 0 || gx >= gridWidth || gy >= gridHeight)
                    return false;

                // 이미 다른 아이템이 차지하고 있으면 불가
                if (grid[gx, gy] != null)
                    return false;
            }
        }
        return true;
    }

    // 아이템 배치
    public void PlaceItem(ItemData item, int x, int y)
    {
        RemoveItem(item); // 기존 위치 제거

        for (int i = 0; i < item.width; i++)
            for (int j = 0; j < item.height; j++)
                grid[x + i, y + j] = item;

        itemPositions[item] = new Vector2Int(x, y);
    }

    // 아이템 제거
    public void RemoveItem(ItemData item)
    {
        for (int x = 0; x < gridWidth; x++)
            for (int y = 0; y < gridHeight; y++)
                if (grid[x, y] == item)
                    grid[x, y] = null;

        itemPositions.Remove(item);
    }

    // 아이템의 좌표 반환
    public Vector2Int? GetItemPosition(ItemData item)
    {
        if (itemPositions.TryGetValue(item, out var pos))
            return pos;
        return null;
    }

    // 인벤토리 확장 기능
    public void ExpandGrid(int addWidth, int addHeight)
    {
        int newWidth = gridWidth + addWidth;
        int newHeight = gridHeight + addHeight;

        ItemData[,] newGrid = new ItemData[newWidth, newHeight];

        // 기존 아이템 복사
        for (int x = 0; x < gridWidth; x++)
            for (int y = 0; y < gridHeight; y++)
                newGrid[x, y] = grid[x, y];

        gridWidth = newWidth;
        gridHeight = newHeight;
        grid = newGrid;

        Debug.Log($"[BackpackGrid] 인벤토리 확장됨 → {gridWidth} x {gridHeight}");

        OnGridExpanded?.Invoke(); // UI 갱신 요청
    }

    // 에디터에서만 그리드 선을 표시
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 1f, 1f, 0.3f); // 반투명 흰색

        float cellSize = 64f; // UI 한 칸 크기

        // 전체 그리드 크기
        float width = gridWidth * cellSize;
        float height = gridHeight * cellSize;

        // 그리드 시작 위치 (GridParent 기준)
        Vector3 origin = transform.position;

        // 세로줄
        for (int x = 0; x <= gridWidth; x++)
        {
            Vector3 start = origin + new Vector3(x * cellSize, 0, 0);
            Vector3 end = origin + new Vector3(x * cellSize, height, 0);
            Gizmos.DrawLine(start, end);
        }

        // 가로줄
        for (int y = 0; y <= gridHeight; y++)
        {
            Vector3 start = origin + new Vector3(0, y * cellSize, 0);
            Vector3 end = origin + new Vector3(width, y * cellSize, 0);
            Gizmos.DrawLine(start, end);
        }
    }

}
