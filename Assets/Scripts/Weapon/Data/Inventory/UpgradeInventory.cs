using System.Collections.Generic;
using UnityEngine;

public class UpgradeInventory : MonoBehaviour
{
    public Inventory inventory;
    public InventoryUI inventoryUI;

    public void UpgradeBottom(int amount = 1)
    {
        Upgrade(inventory.gridWidth, inventory.gridHeight + amount, Direction.Bottom);
    }

    public void UpgradeTop(int amount = 1)
    {
        Upgrade(inventory.gridWidth, inventory.gridHeight + amount, Direction.Top);
    }

    public void UpgradeRight(int amount = 1)
    {
        Upgrade(inventory.gridWidth + amount, inventory.gridHeight, Direction.Right);
    }

    public void UpgradeLeft(int amount = 1)
    {
        Upgrade(inventory.gridWidth + amount, inventory.gridHeight, Direction.Left);
    }

    enum Direction { Top, Bottom, Left, Right }

    void Upgrade(int newWidth, int newHeight, Direction dir)
    {
        // 기존 아이템 백업
        var oldItems = new List<ItemInstance>(inventory.items);

        // 기존 슬롯 위치 저장
        Dictionary<ItemInstance, Vector2Int> oldPositions = inventory.GetItemPositions();

        // 인벤토리 크기 갱신
        inventory.gridWidth = newWidth;
        inventory.gridHeight = newHeight;

        // 새 그리드 생성
        inventory.grid = new InventoryGrid(newWidth, newHeight);
        inventory.items.Clear();

        // 방향에 따라 기존 아이템 좌표 보정
        foreach (var item in oldItems)
        {
            Vector2Int pos = oldPositions[item];

            switch (dir)
            {
                case Direction.Top:
                    pos.y += 1; // 위로 확장 → 기존 아이템 아래로 이동
                    break;

                case Direction.Left:
                    pos.x += 1; // 왼쪽 확장 → 기존 아이템 오른쪽으로 이동
                    break;
            }

            // 새 위치에 다시 배치
            inventory.grid.PlaceItem(item, pos.x, pos.y);
            inventory.items.Add(item);
        }

        inventoryUI.RebuildGrid();
        inventoryUI.RefreshItems();
    }
}
