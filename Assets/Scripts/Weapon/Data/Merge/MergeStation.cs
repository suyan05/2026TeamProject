using UnityEngine;
using System.Collections.Generic;

public class MergeStation : MonoBehaviour
{
    public Transform centerPoint;
    public float radius = 1.5f;
    public float rotateSpeed = 40f;
    public float floatAmplitude = 0.1f;
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

    public void ClearItems()
    {
        foreach (var obj in orbitItems)
            Destroy(obj);

        orbitItems.Clear();
    }

    void UpdateItemPositions()
    {
        int count = orbitItems.Count;
        if (count == 0) return;

        for (int i = 0; i < count; i++)
        {
            float angle = (360f / count) * i;

            Vector3 offset = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad) * radius,
                0,
                Mathf.Sin(angle * Mathf.Deg2Rad) * radius
            );

            orbitItems[i].transform.position = centerPoint.position + offset;
        }
    }

    void RotateItems()
    {
        foreach (var item in orbitItems)
            item.transform.RotateAround(centerPoint.position, Vector3.up, rotateSpeed * Time.deltaTime);
    }

    void FloatEffect()
    {
        foreach (var item in orbitItems)
        {
            Vector3 pos = item.transform.position;
            pos.y = centerPoint.position.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            item.transform.position = pos;
        }
    }
}
