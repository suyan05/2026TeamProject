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

        ItemInstance instance = dragged.itemInstance;

        if (inventory.grid.CanPlaceItem(instance, x, y))
        {
            inventory.grid.RemoveItem(instance);
            inventory.grid.PlaceItem(instance, x, y);
        }

        inventoryUI.RefreshItems();
    }
}
