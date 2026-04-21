using UnityEngine;
using System.Collections.Generic;

public class MergeStationUI : MonoBehaviour
{
    public RectTransform centerPoint;
    public float radius = 150f;
    public float rotateSpeed = 40f;

    public List<RectTransform> slotList = new List<RectTransform>();

    void Update()
    {
        RotateSlots();
    }

    public void AddSlot(RectTransform slot)
    {
        slotList.Add(slot);
        UpdateSlotPositions();
    }

    public void ClearSlots()
    {
        foreach (var s in slotList)
            Destroy(s.gameObject);

        slotList.Clear();
    }

    void UpdateSlotPositions()
    {
        int count = slotList.Count;
        if (count == 0) return;

        for (int i = 0; i < count; i++)
        {
            float angle = (360f / count) * i;
            Vector2 pos = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad) * radius,
                Mathf.Sin(angle * Mathf.Deg2Rad) * radius
            );

            slotList[i].anchoredPosition = pos;
        }
    }

    void RotateSlots()
    {
        foreach (var slot in slotList)
            slot.RotateAround(centerPoint.position, Vector3.forward, rotateSpeed * Time.deltaTime);
    }
}
