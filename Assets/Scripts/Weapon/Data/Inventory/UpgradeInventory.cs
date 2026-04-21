using UnityEngine;

public class UpgradeInventory : MonoBehaviour
{
    public Inventory inventory;
    public InventoryUI inventoryUI;

    public void Upgrade(int newWidth, int newHeight)
    {
        // 인벤토리 크기 변경
        inventory.gridWidth = newWidth;
        inventory.gridHeight = newHeight;

        // UI 그리드 재생성 + 아이템 다시 배치
        inventoryUI.RebuildGrid();
        inventoryUI.RefreshItems();
    }
}