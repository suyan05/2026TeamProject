using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    // ?? 싱글톤 인스턴스 추가 (상점 코드가 에러 없이 안전하게 찾을 수 있도록 방어)
    public static Inventory Instance { get; private set; }

    public int gridWidth = 6;
    public int gridHeight = 6;

    public InventoryGrid grid;
    public List<ItemInstance> items = new List<ItemInstance>();

    void Awake()
    {
        // ?? 싱글톤 초기화
        if (Instance == null) Instance = this;

        // ?? [핵심 에러 차단] grid가 비어있다면 여기서 확실하게 먼저 생성해 줍니다.
        if (grid == null)
        {
            grid = new InventoryGrid(gridWidth, gridHeight);
        }
    }

    public bool TryAddItem(ItemData data)
    {
        // ?? 2중 안전장치: 혹시나 어떤 이유로든 grid가 아직도 Null이라면 강제 생성해서 에러를 막습니다.
        if (grid == null)
        {
            grid = new InventoryGrid(gridWidth, gridHeight);
        }

        if (items == null)
        {
            items = new List<ItemInstance>();
        }

        ItemInstance instance = new ItemInstance(data);

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                // ?? 기존 격자 배치 검사 로직 그대로 유지!
                if (grid.CanPlaceItem(instance, x, y))
                {
                    grid.PlaceItem(instance, x, y);
                    items.Add(instance);

                    // ?? 시현님의 기존 스탯 재계산 로직 그대로 유지!
                    if (PlayerMovement.Instance != null)
                    {
                        PlayerMovement.Instance.RecalculateStats(items);
                    }
                    return true;
                }
            }
        }
        return false;
    }

    public void RemoveItem(ItemInstance instance)
    {
        if (grid != null) grid.RemoveItem(instance);
        if (items != null) items.Remove(instance);

        if (PlayerMovement.Instance != null)
        {
            PlayerMovement.Instance.RecalculateStats(items);
        }
    }

    public Dictionary<ItemInstance, Vector2Int> GetItemPositions()
    {
        Dictionary<ItemInstance, Vector2Int> dict = new Dictionary<ItemInstance, Vector2Int>();

        if (items == null || grid == null || grid.slots == null) return dict;

        foreach (var item in items)
        {
            for (int y = 0; y < grid.gridHeight; y++)
            {
                for (int x = 0; x < grid.gridWidth; x++)
                {
                    var slot = grid.slots[x, y];
                    if (slot != null && slot.item != null && slot.item.uniqueID == item.uniqueID)
                    {
                        dict[item] = new Vector2Int(x, y);
                        goto Found;
                    }
                }
            }
        Found:;
        }

        return dict;
    }

    public Vector2Int FindClosestEmptySlot(Vector2 screenPosition, InventoryUI ui)
    {
        float bestDist = float.MaxValue;
        Vector2Int bestSlot = new Vector2Int(-1, -1);

        if (grid == null || grid.slots == null || ui == null || ui.slotUIs == null) return bestSlot;

        for (int y = 0; y < grid.gridHeight; y++)
        {
            for (int x = 0; x < grid.gridWidth; x++)
            {
                if (grid.slots[x, y] != null) continue; // 이미 차있으면 패스

                // 슬롯 UI의 화면 좌표 가져오기
                RectTransform slotRect = ui.slotUIs[x, y].GetComponent<RectTransform>();
                if (slotRect == null) continue;

                Vector2 slotScreenPos = RectTransformUtility.WorldToScreenPoint(null, slotRect.position);

                float dist = Vector2.Distance(screenPosition, slotScreenPos);

                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestSlot = new Vector2Int(x, y);
                }
            }
        }

        return bestSlot;
    }
}