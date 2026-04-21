using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class MergeDropZone : MonoBehaviour, IDropHandler
{
    public MergeStation mergeStation;
    public MergeManager mergeManager;

    private List<ItemData> selectedItems = new List<ItemData>();

    public void OnDrop(PointerEventData eventData)
    {
        InventoryItemUI itemUI = eventData.pointerDrag.GetComponent<InventoryItemUI>();
        if (itemUI == null) return;

        selectedItems.Add(itemUI.itemData);

        GameObject worldObj = Instantiate(itemUI.itemData.worldPrefab);
        mergeStation.AddItem(worldObj);

        Destroy(itemUI.gameObject);
    }

    public void TryMerge()
    {
        mergeManager.TryMerge(selectedItems);

        selectedItems.Clear();
        mergeStation.ClearItems();
    }
}
