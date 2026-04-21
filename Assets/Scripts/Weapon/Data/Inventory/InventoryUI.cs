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

    // 그리드 재생성
    public void RebuildGrid()
    {
        foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        GenerateGrid();
    }

    // 슬롯 배경 생성 (0,0 중심 기준)
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

    // 아이템 UI 생성
    public void RefreshItems()
    {
        // 기존 UI 제거
        foreach (Transform child in itemParent)
            Destroy(child.gameObject);

        // 인벤토리에 있는 모든 아이템 UI 생성
        foreach (var item in inventory.items)
        {
            int foundX = -1;
            int foundY = -1;

            // 아이템이 차지한 칸 중 "가장 왼쪽 위 칸" 찾기
            for (int y = 0; y < inventory.grid.gridHeight; y++)
            {
                for (int x = 0; x < inventory.grid.gridWidth; x++)
                {
                    var slot = inventory.grid.slots[x, y];
                    if (slot != null && slot.item == item)
                    {
                        foundX = x;
                        foundY = y;
                        goto Found; // 첫 번째 발견된 칸이 곧 아이템의 기준 위치
                    }
                }
            }
        Found:

            // 아이템이 실제로 차지한 칸이 없다면 스킵
            if (foundX == -1)
                continue;

            // UI 생성
            GameObject itemObj = Instantiate(itemPrefab, itemParent);
            InventoryItemUI itemUI = itemObj.GetComponent<InventoryItemUI>();

            itemUI.inventory = inventory;
            itemUI.inventoryUI = this;
            itemUI.SetItem(item);

            RectTransform rt = itemObj.GetComponent<RectTransform>();
            RectTransform slotRT = slotUIs[foundX, foundY].GetComponent<RectTransform>();

            // 아이템 크기 계산
            float w = item.width * slotSize;
            float h = item.height * slotSize;

            // 슬롯 중심과 아이템 중심 차이 보정
            float offsetX = (w - slotSize) / 2f;
            float offsetY = (h - slotSize) / 2f;

            // 최종 위치 적용
            rt.anchoredPosition = slotRT.anchoredPosition + new Vector2(offsetX, -offsetY);

            // 크기 적용
            rt.sizeDelta = new Vector2(w, h);
        }
    }
}
