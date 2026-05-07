using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EquipmentSlotUI : MonoBehaviour, IDropHandler
{
    public ItemInstance equippedItem;

    public Inventory inventory;
    public InventoryUI inventoryUI;

    private GameObject currentItemUI;

    public void OnDrop(PointerEventData eventData)
    {
        InventoryItemUI dragged = eventData.pointerDrag.GetComponent<InventoryItemUI>();
        if (dragged == null) return;

        ItemInstance item = dragged.itemInstance;

        // 무기만 장착 가능
        if (item.data.itemType != ItemType.Weapon)
            return;

        // 장비칸에 드롭되었다고 표시
        dragged.droppedOnEquipment = true;

        // 인벤토리에서 먼저 제거
        inventory.RemoveItem(item);
        inventoryUI.RefreshItems();

        // 장비칸에 장착
        Equip(item);
    }

    public void Equip(ItemInstance newItem)
    {
        // 기존 장착 아이템이 있으면 인벤토리로 되돌리기
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
        if (equippedItem == null) return;

        if (!inventory.TryAddItem(equippedItem.data))
        {
            Debug.LogWarning("인벤토리가 가득 차서 장비를 해제할 수 없습니다.");
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
