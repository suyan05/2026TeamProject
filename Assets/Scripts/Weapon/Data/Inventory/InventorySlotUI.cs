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

        // 원래 위치 제거
        inventory.grid.RemoveItem(instance);

        // 새 위치에 배치
        if (inventory.grid.CanPlaceItem(instance, x, y))
        {
            inventory.grid.PlaceItem(instance, x, y);
        }
        else
        {
            // 배치 불가 원래 자리로 복구
            inventory.grid.PlaceItem(instance, dragged.originalX, dragged.originalY);
        }

        inventoryUI.RefreshItems();
    }

}
