using UnityEngine;
using UnityEngine.EventSystems;

public class MergeDropZone : MonoBehaviour, IDropHandler
{
    public MergeStationUI station;
    public Inventory inventory;
    public InventoryUI inventoryUI;

    private void Awake()
    {
        if (station == null) station = FindObjectOfType<MergeStationUI>();
        if (inventory == null) inventory = FindObjectOfType<Inventory>();
        if (inventoryUI == null) inventoryUI = FindObjectOfType<InventoryUI>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        // 드래그된 UI가 InventoryItemUI인지 확인
        InventoryItemUI dragged = eventData.pointerDrag?.GetComponent<InventoryItemUI>();
        if (dragged == null) return;

        ItemInstance instance = dragged.itemInstance;
        if (instance == null) return;

        // 1) 인벤토리에서 제거
        inventory.grid.RemoveItem(instance);
        inventory.items.Remove(instance);

        // 2) 머지 슬롯에 추가
        station.AddItem(instance);

        // 3) 인벤토리 UI 갱신
        inventoryUI.RefreshItems();
    }
}
