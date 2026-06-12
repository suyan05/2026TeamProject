using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EquipmentSlotUI : MonoBehaviour, IDropHandler
{
    public ItemInstance equippedItem;

    public Inventory inventory;
    public InventoryUI inventoryUI;

    private GameObject currentItemUI;

    bool isFinding = true;

    private void Start()
    {
        TryFindReferences();
    }

    private void Update()
    {
        if (isFinding)
            TryFindReferences();
    }

    void TryFindReferences()
    {
        if (inventory == null)
            inventory = Object.FindFirstObjectByType<Inventory>(FindObjectsInactive.Include);

        if (inventoryUI == null)
        {
            // 1차: 일반 검색
            inventoryUI = Object.FindFirstObjectByType<InventoryUI>(FindObjectsInactive.Include);

            // 2차: DontDestroyOnLoad까지 포함한 전체 검색
            if (inventoryUI == null)
            {
                var all = Resources.FindObjectsOfTypeAll<InventoryUI>();
                if (all != null && all.Length > 0)
                    inventoryUI = all[0];
            }
        }

        // 둘 다 찾았으면 탐색 종료
        if (inventory != null && inventoryUI != null)
        {
            isFinding = false;
            Debug.Log("<color=lime>[EquipmentSlotUI] Inventory & InventoryUI 자동 참조 완료!</color>");
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (inventory == null || inventoryUI == null)
            return;

        InventoryItemUI dragged = eventData.pointerDrag.GetComponent<InventoryItemUI>();
        if (dragged == null) return;

        ItemInstance item = dragged.itemInstance;

        if (item.data.itemType != ItemType.Weapon)
            return;

        dragged.droppedOnEquipment = true;

        inventory.RemoveItem(item);
        inventoryUI.RefreshItems();

        Equip(item);
    }

    public void Equip(ItemInstance newItem)
    {
        if (inventory == null || inventoryUI == null)
            return;

        if (equippedItem != null)
        {
            inventory.TryAddItem(equippedItem.data);
            inventoryUI.RefreshItems();
        }

        equippedItem = newItem;

        if (currentItemUI != null)
            Destroy(currentItemUI);

        GameObject prefab = inventoryUI.itemPrefab;
        currentItemUI = Instantiate(prefab, transform);

        InventoryItemUI ui = currentItemUI.GetComponent<InventoryItemUI>();
        ui.itemInstance = newItem;
        ui.inventory = inventory;
        ui.inventoryUI = inventoryUI;
        ui.icon.sprite = newItem.data.icon;

        PlayerMovement.Instance.EquipWeapon(newItem.data);
        PlayerMovement.Instance.RecalculateStats(inventory.items);

        UpdateUI();
    }

    public void UnequipToInventory()
    {
        if (inventory == null || inventoryUI == null)
            return;

        if (equippedItem == null) return;

        if (!inventory.TryAddItem(equippedItem.data))
        {
            Debug.LogWarning("인벤토리에 공간이 부족하여 장비를 해제할 수 없습니다.");
            return;
        }

        PlayerMovement.Instance.EquipWeapon(null);

        equippedItem = null;

        if (currentItemUI != null)
            Destroy(currentItemUI);
    }

    private void UpdateUI()
    {
        if (currentItemUI != null)
            Destroy(currentItemUI);

        if (equippedItem == null)
            return;

        GameObject prefab = inventoryUI.itemPrefab;
        currentItemUI = Instantiate(prefab, transform);

        InventoryItemUI ui = currentItemUI.GetComponent<InventoryItemUI>();
        ui.itemInstance = equippedItem;
        ui.inventory = inventory;
        ui.inventoryUI = inventoryUI;
        ui.icon.sprite = equippedItem.data.icon;
    }
}
