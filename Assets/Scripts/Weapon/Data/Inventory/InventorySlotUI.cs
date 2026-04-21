using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour, IDropHandler
{
    public int x;
    public int y;

    public Inventory inventory;
    public InventoryUI inventoryUI;

    public void OnDrop(PointerEventData eventData)
    {
        InventoryItemUI dragged = eventData.pointerDrag.GetComponent<InventoryItemUI>();
        if (dragged == null) return;

        ItemData item = dragged.itemData;

        if (inventory.grid.CanPlaceItem(item, x, y))
        {
            inventory.RemoveItem(item);
            inventory.grid.PlaceItem(item, x, y);
        }
        else
        {
            inventory.grid.PlaceItem(item, dragged.originalX, dragged.originalY);
        }

        inventory.grid.DebugPrintGrid();

        inventoryUI.RefreshItems();
    }
}
