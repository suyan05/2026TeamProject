using UnityEngine;
using System.Collections.Generic;
using System;

public class Inventory : MonoBehaviour
{
    [Header("플레이어가 보유한 아이템들")]
    [SerializeField]
    private List<ItemData> items = new List<ItemData>();
    public IReadOnlyList<ItemData> Items => items;

    [Header("최대 보유 아이템 개수 (0이면 무제한)")]
    public int maxCapacity = 0;

    // 인벤토리 변경 시 호출되는 이벤트 (옵션)
    public event Action OnInventoryChanged;

    // 아이템 추가
    public bool AddItem(ItemData item)
    {
        if (item == null) return false;
        if (maxCapacity > 0 && items.Count >= maxCapacity)
        {
            Debug.Log("인벤토리가 가득 찼습니다.");
            return false;
        }
        items.Add(item);
        OnInventoryChanged?.Invoke();
        return true;
    }

    // 아이템 제거
    public bool RemoveItem(ItemData item)
    {
        if (item == null) return false;
        bool removed = items.Remove(item);
        if (removed)
            OnInventoryChanged?.Invoke();
        return removed;
    }

    // 아이템 보유 여부 확인
    public bool Contains(ItemData item)
    {
        return items.Contains(item);
    }

    // 아이템 전체 제거
    public void Clear()
    {
        items.Clear();
        OnInventoryChanged?.Invoke();
    }
}
