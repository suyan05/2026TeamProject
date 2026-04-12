using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public Inventory inventory;
    public InventoryGrid grid;

    [Header("아이템 UI 프리팹")]
    public GameObject itemUIPrefab;

    [Header("그리드 UI 부모 오브젝트")]
    public Transform gridParent;

    void OnEnable()
    {
        inventory.OnInventoryChanged += RefreshUI;
        RefreshUI();
    }

    void OnDisable()
    {
        inventory.OnInventoryChanged -= RefreshUI;
    }

    // 인벤토리 변경 시 UI 갱신
    void RefreshUI()
    {
        foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        foreach (var item in inventory.Items)
        {
            GameObject ui = Instantiate(itemUIPrefab, gridParent);
            InventoryItemUI uiItem = ui.GetComponent<InventoryItemUI>();

            uiItem.itemData = item;
            uiItem.grid = grid;
            uiItem.inventory = inventory;

            ui.GetComponentInChildren<UnityEngine.UI.Image>().sprite = item.icon;
        }
    }
}
