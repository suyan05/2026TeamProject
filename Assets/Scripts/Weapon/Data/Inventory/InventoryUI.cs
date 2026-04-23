using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public Inventory inventory;

    public RectTransform gridParent;
    public RectTransform itemParent;

    public GameObject slotPrefab;
    public GameObject itemPrefab;

    public int slotSize = 64;

    public InventorySlotUI[,] slotUIs;

    void Start()
    {
        RebuildGrid();
        RefreshItems();
    }

    public void RebuildGrid()
    {
        foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        GenerateGrid();
    }

    void GenerateGrid()
    {
        slotUIs = new InventorySlotUI[inventory.gridWidth, inventory.gridHeight];

        float centerX = (inventory.gridWidth * slotSize) / 2f;
        float centerY = (inventory.gridHeight * slotSize) / 2f;

        for (int y = 0; y < inventory.gridHeight; y++)
        {
            for (int x = 0; x < inventory.gridWidth; x++)
            {
                GameObject slotObj = Instantiate(slotPrefab, gridParent);
                RectTransform rt = slotObj.GetComponent<RectTransform>();

                float posX = (x * slotSize) - centerX + slotSize / 2f;
                float posY = centerY - (y * slotSize) - slotSize / 2f;

                rt.anchoredPosition = new Vector2(posX, posY);

                InventorySlotUI slotUI = slotObj.GetComponent<InventorySlotUI>();
                slotUI.x = x;
                slotUI.y = y;
                slotUI.inventory = inventory;
                slotUI.inventoryUI = this;

                slotUIs[x, y] = slotUI;
            }
        }
    }

    public void RefreshItems()
    {
        foreach (Transform child in itemParent)
            Destroy(child.gameObject);

        foreach (var instance in inventory.items)
        {
            int foundX = -1;
            int foundY = -1;

            for (int y = 0; y < inventory.grid.gridHeight; y++)
            {
                for (int x = 0; x < inventory.grid.gridWidth; x++)
                {
                    var slot = inventory.grid.slots[x, y];
                    if (slot != null && slot.item.uniqueID == instance.uniqueID)
                    {
                        foundX = x;
                        foundY = y;
                        goto Found;
                    }
                }
            }
        Found:

            if (foundX == -1)
                continue;

            GameObject itemObj = Instantiate(itemPrefab, itemParent);
            InventoryItemUI itemUI = itemObj.GetComponent<InventoryItemUI>();

            itemUI.inventory = inventory;
            itemUI.inventoryUI = this;
            itemUI.SetItem(instance);

            RectTransform rt = itemObj.GetComponent<RectTransform>();
            RectTransform slotRT = slotUIs[foundX, foundY].GetComponent<RectTransform>();

            float w = instance.data.width * slotSize;
            float h = instance.data.height * slotSize;

            float offsetX = (w - slotSize) / 2f;
            float offsetY = (h - slotSize) / 2f;

            rt.anchoredPosition = slotRT.anchoredPosition + new Vector2(offsetX, -offsetY);
            rt.sizeDelta = new Vector2(w, h);
        }
    }
}
