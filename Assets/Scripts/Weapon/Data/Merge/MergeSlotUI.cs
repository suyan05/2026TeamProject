using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MergeSlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public ItemInstance itemInstance;
    public Image icon;

    public GameObject itemPrefab; // InventoryItemUI 프리팹

    private GameObject dragUI;
    private RectTransform dragRT;

    private MergeStationUI station;

    private void Awake()
    {
        station = FindObjectOfType<MergeStationUI>();
    }

    public void SetItem(ItemInstance instance)
    {
        itemInstance = instance;
        icon.sprite = instance.data.icon;
        icon.enabled = true;

        // 아이템 비율 반영
        RectTransform rt = GetComponent<RectTransform>();

        float baseSize = 64; // 머지 스테이션에서 기본 슬롯 크기
        float w = instance.data.width * baseSize;
        float h = instance.data.height * baseSize;

        rt.sizeDelta = new Vector2(w, h);
    }


    public void Clear()
    {
        itemInstance = null;
        icon.enabled = false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (itemInstance == null) return;

        dragUI = Instantiate(itemPrefab, transform.root);
        dragRT = dragUI.GetComponent<RectTransform>();

        InventoryItemUI ui = dragUI.GetComponent<InventoryItemUI>();
        ui.inventory = FindObjectOfType<Inventory>();
        ui.inventoryUI = FindObjectOfType<InventoryUI>();
        ui.SetItem(itemInstance);

        CanvasGroup cg = dragUI.GetComponent<CanvasGroup>();
        if (cg == null) cg = dragUI.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragRT != null)
            dragRT.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragUI != null)
            Destroy(dragUI);

        InventorySlotUI slot = eventData.pointerEnter?.GetComponent<InventorySlotUI>();

        if (slot != null)
        {
            var inv = slot.inventory;
            var grid = inv.grid;

            if (grid.CanPlaceItem(itemInstance, slot.x, slot.y))
            {
                grid.PlaceItem(itemInstance, slot.x, slot.y);
                inv.items.Add(itemInstance);

                station.RemoveItemFromSlot(this);
                Destroy(gameObject);

                slot.inventoryUI.RefreshItems();
                return;
            }
        }
    }
}
