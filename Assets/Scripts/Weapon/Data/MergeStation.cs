// 합성대에서 아이템이 원형 회전 + Glow 이펙트
using UnityEngine;
using System.Collections.Generic;

public class MergeStation : MonoBehaviour
{
    public Transform centerPoint;
    public float radius = 1.5f;
    public float rotateSpeed = 50f;
    public float floatAmplitude = 0.1f;  // 위아래 흔들림
    public float floatSpeed = 2f;

    private List<GameObject> orbitItems = new List<GameObject>();

    void Update()
    {
        RotateItems();
        FloatEffect();
    }

    public void AddItem(GameObject item)
    {
        orbitItems.Add(item);
        UpdateItemPositions();
    }

    public void RemoveItem(GameObject item)
    {
        orbitItems.Remove(item);
        UpdateItemPositions();
    }

    void UpdateItemPositions()
    {
        int count = orbitItems.Count;
        if (count == 0) return;

        for (int i = 0; i < count; i++)
        {
            float angle = (360f / count) * i;
            Vector3 pos = centerPoint.position +
                          Quaternion.Euler(0, 0, angle) * Vector3.right * radius;

            orbitItems[i].transform.position = pos;
        }
    }

    void RotateItems()
    {
        foreach (var item in orbitItems)
            item.transform.RotateAround(centerPoint.position, Vector3.forward, rotateSpeed * Time.deltaTime);
    }

    void FloatEffect()
    {
        foreach (var item in orbitItems)
        {
            Vector3 pos = item.transform.position;
            pos.y += Mathf.Sin(Time.time * floatSpeed) * floatAmplitude * Time.deltaTime;
            item.transform.position = pos;
        }
    }
}