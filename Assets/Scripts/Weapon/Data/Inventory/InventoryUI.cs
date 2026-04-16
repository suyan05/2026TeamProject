using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public Inventory inventory;
    public InventoryGrid grid;

    [Header("아이템 UI 프리팹")]
    public GameObject itemUIPrefab;

    [Header("아이템들이 배치될 부모 오브젝트")]
    public Transform gridParent;

    [Header("자동 배치 딜레이(초)")]
    public float autoPlaceDelay = 0.2f;

    void OnEnable()
    {
        if (inventory != null)
            inventory.OnInventoryChanged += RefreshUI;
        if (grid != null)
            grid.OnGridExpanded += RefreshUI;
        RefreshUI();
    }

    void OnDisable()
    {
        if (inventory != null)
            inventory.OnInventoryChanged -= RefreshUI;
        if (grid != null)
            grid.OnGridExpanded -= RefreshUI;
        StopAllCoroutines();
    }

    void ResizeGridUI()
    {
        if (gridParent == null || grid == null) return;
        RectTransform rt = gridParent.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(
            grid.gridWidth * 64f,
            grid.gridHeight * 64f
        );
    }

    void RefreshUI()
    {
        ResizeGridUI();

        foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        if (inventory == null || grid == null || itemUIPrefab == null) return;

        foreach (var item in inventory.Items)
        {
            GameObject ui = Instantiate(itemUIPrefab, gridParent);
            ui.SetActive(true); // 오브젝트 강제 활성화

            InventoryItemUI uiItem = ui.GetComponent<InventoryItemUI>();
            if (uiItem != null)
            {
                uiItem.Init(item, grid, inventory);
            }

            var img = ui.GetComponentInChildren<Image>();
            if (img != null)
                img.sprite = item.icon;

            var pos = grid.GetItemPosition(item);
            if (pos.HasValue)
            {
                var rect = ui.GetComponent<RectTransform>();
                if (rect != null)
                    rect.anchoredPosition = new Vector2(pos.Value.x * 64f, pos.Value.y * 64f);
            }
        }
    }
}
