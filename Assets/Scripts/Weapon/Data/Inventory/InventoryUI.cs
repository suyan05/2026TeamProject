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

    public RectTransform inventoryArea; // 인벤토리 전체 영역

    void Start()
    {
        // ?? 안전장치: 혹시라도 inventory 연결이 끊겨있다면 자동으로 찾아줍니다.
        if (inventory == null) inventory = FindObjectOfType<Inventory>(true);

        RebuildGrid();
        RefreshItems();
    }

    public void RebuildGrid()
    {
        // ?? 안전장치: inventory 데이터가 아예 없으면 그리드를 그리지 않고 대기합니다.
        if (inventory == null) inventory = FindObjectOfType<Inventory>(true);
        if (inventory == null) return;

        foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        GenerateGrid();
    }

    void GenerateGrid()
    {
        if (inventory == null) return;

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
        if (inventory == null || inventory.items == null || inventory.grid == null) return;

        foreach (Transform child in itemParent)
            Destroy(child.gameObject);

        // ?? [?? 핵심 에러 차단 부품]: 만약 slotUIs 배열판이 아직 안 만들어졌다면 강제로 먼저 만들어줍니다.
        if (slotUIs == null)
        {
            GenerateGrid();
        }

        foreach (var instance in inventory.items)
        {
            if (instance == null || instance.data == null) continue;

            int foundX = -1;
            int foundY = -1;

            for (int y = 0; y < inventory.grid.gridHeight; y++)
            {
                for (int x = 0; x < inventory.grid.gridWidth; x++)
                {
                    var slot = inventory.grid.slots[x, y];
                    // uniqueID 비교 시 발생할 수 있는 Null 예외 방어 추가
                    if (slot != null && slot.item != null && slot.item.uniqueID == instance.uniqueID)
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

            // ?? [?? 99번째 줄 폭발 방지선]: 찾은 슬롯 위치의 UI 컴포넌트가 비어있다면 에러를 내지 않고 넘어갑니다.
            if (slotUIs[foundX, foundY] == null)
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

            // 시현님의 원래 배치 공식 정교하게 100% 유지
            rt.anchoredPosition = slotRT.anchoredPosition + new Vector2(offsetX, -offsetY);
            rt.sizeDelta = new Vector2(w, h);
        }
    }
}