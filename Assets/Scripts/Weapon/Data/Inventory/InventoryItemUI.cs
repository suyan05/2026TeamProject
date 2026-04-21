using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public ItemData itemData;
    public Inventory inventory;
    public InventoryUI inventoryUI;

    public Image icon;

    public int originalX;
    public int originalY;

    Transform originalParent;

    public void SetItem(ItemData data)
    {
        itemData = data;
        icon.sprite = data.icon;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;

        // 원래 좌표 찾기
        for (int y = 0; y < inventory.grid.gridHeight; y++)
        {
            for (int x = 0; x < inventory.grid.gridWidth; x++)
            {
                var slot = inventory.grid.slots[x, y];
                if (slot != null && slot.item == itemData)
                {
                    originalX = x;
                    originalY = y;
                    goto Found;
                }
            }
        }
    Found:

        transform.SetParent(transform.root);
        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(originalParent);
        GetComponent<CanvasGroup>().blocksRaycasts = true;

        inventoryUI.RefreshItems();
    }
}
