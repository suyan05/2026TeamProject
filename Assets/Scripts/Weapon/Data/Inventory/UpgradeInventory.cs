using UnityEngine;

public class UpgradeInventory : MonoBehaviour
{
    public Inventory inventory;
    public InventoryUI inventoryUI;

    // 기존 업그레이드 함수
    public void Upgrade(int newWidth, int newHeight)
    {
        inventory.gridWidth = newWidth;
        inventory.gridHeight = newHeight;

        inventoryUI.RebuildGrid();
        inventoryUI.RefreshItems();
    }

    // 버튼에서 호출할 함수
    public void UpgradeBy4()
    {
        int newHeight = inventory.gridHeight + 1;
        Upgrade(inventory.gridWidth, newHeight);
    }
}
