using UnityEngine;
using System.Collections.Generic;

public class MergeStationUI : MonoBehaviour
{
    public Transform slotContainer;
    public GameObject mergeSlotPrefab;

    public List<MergeSlotUI> slots = new List<MergeSlotUI>();

    public float radius = 150f;     // 원형 반경
    public float rotateSpeed = 20f; // 회전 속도 (도/초)

    private float currentAngle = 0f;

    private void Update()
    {
        if (slots.Count > 0)
            RotateSlots();
    }

    public void AddItem(ItemInstance instance)
    {
        GameObject obj = Instantiate(mergeSlotPrefab, slotContainer);
        MergeSlotUI slot = obj.GetComponent<MergeSlotUI>();
        slot.SetItem(instance);

        slots.Add(slot);

        AutoArrangeSlots(); // 초기 배치
    }

    public void RemoveItemFromSlot(MergeSlotUI slot)
    {
        slots.Remove(slot);
        AutoArrangeSlots();
    }

    public void ClearAll()
    {
        foreach (var s in slots)
            Destroy(s.gameObject);

        slots.Clear();
    }

    // 초기 배치 (정지 상태)
    public void AutoArrangeSlots()
    {
        if (slots.Count == 0) return;

        float angleStep = 360f / slots.Count;

        for (int i = 0; i < slots.Count; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;

            Vector2 pos = new Vector2(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius
            );

            RectTransform rt = slots[i].GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
        }
    }

    // 지속 회전
    private void RotateSlots()
    {
        currentAngle += rotateSpeed * Time.deltaTime;

        float angleStep = 360f / slots.Count;

        for (int i = 0; i < slots.Count; i++)
        {
            float angle = (angleStep * i + currentAngle) * Mathf.Deg2Rad;

            Vector2 pos = new Vector2(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius
            );

            RectTransform rt = slots[i].GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
        }
    }
}
