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

        // 새 위치에 배치 가능한지 먼저 검사
        if (inventory.grid.CanPlaceItem(item, x, y))
        {
            inventory.RemoveItem(item);
            inventory.grid.PlaceItem(item, x, y);
        }
        else
        {
            // 배치 불가 -> 원래 자리로 복구
            inventory.grid.PlaceItem(item, dragged.originalX, dragged.originalY);
        }

        inventoryUI.RefreshItems();
    }
}
