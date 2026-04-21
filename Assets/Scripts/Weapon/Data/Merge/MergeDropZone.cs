using UnityEngine;
using UnityEngine.EventSystems;

public class MergeDropZone : MonoBehaviour, IDropHandler
{
    public MergeStationUI station;
    public GameObject mergeSlotPrefab;
    public RectTransform slotContainer;

    public void OnDrop(PointerEventData eventData)
    {
        InventoryItemUI itemUI = eventData.pointerDrag.GetComponent<InventoryItemUI>();
        if (itemUI == null) return;

        GameObject slotObj = Instantiate(mergeSlotPrefab, slotContainer);
        MergeSlotUI slot = slotObj.GetComponent<MergeSlotUI>();
        slot.SetItem(itemUI.itemData);

        station.AddSlot(slotObj.GetComponent<RectTransform>());

        itemUI.inventory.RemoveItem(itemUI.itemData);
        Destroy(itemUI.gameObject);
    }
}
