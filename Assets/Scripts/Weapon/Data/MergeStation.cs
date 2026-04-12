// ---------------------------------------------------------
// MergeStation
// - 합성대에 올려진 아이템들이 중앙을 기준으로 원형 회전
// - 위아래로 부드럽게 흔들리는 이펙트 포함
// ---------------------------------------------------------
using UnityEngine;
using System.Collections.Generic;

public class MergeStation : MonoBehaviour
{
    public Transform centerPoint;       // 회전 중심
    public float radius = 1.5f;         // 회전 반경
    public float rotateSpeed = 40f;     // 회전 속도
    public float floatAmplitude = 0.1f; // 위아래 흔들림 크기
    public float floatSpeed = 2f;       // 흔들림 속도

    private List<GameObject> orbitItems = new List<GameObject>();

    void Update()
    {
        RotateItems();
        FloatEffect();
    }

    // 아이템 추가
    public void AddItem(GameObject item)
    {
        orbitItems.Add(item);
        UpdateItemPositions();
    }

    // 아이템 제거
    public void RemoveItem(GameObject item)
    {
        orbitItems.Remove(item);
        UpdateItemPositions();
    }

    // 3D 원형 배치
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

    // Y축 기준 회전
    void RotateItems()
    {
        foreach (var item in orbitItems)
            item.transform.RotateAround(centerPoint.position, Vector3.up, rotateSpeed * Time.deltaTime);
    }

    // 위아래 흔들림
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
