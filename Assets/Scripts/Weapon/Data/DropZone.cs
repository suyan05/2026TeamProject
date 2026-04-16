using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    public MergeStation station;
    public Transform spawnPoint;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData == null || eventData.pointerDrag == null)
            return;

        var dropped = eventData.pointerDrag.GetComponent<InventoryItemUI>();
        if (dropped == null)
            return;

        if (station == null)
        {
            Debug.LogWarning("MergeStation이 할당되지 않았습니다.", this);
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogWarning("spawnPoint가 할당되지 않았습니다.", this);
            return;
        }

        if (dropped.itemData == null)
        {
            Debug.LogWarning("드롭된 오브젝트에 ItemData가 없습니다.", dropped);
            return;
        }

        if (dropped.itemData.worldPrefab == null)
        {
            Debug.LogWarning("ItemData에 worldPrefab이 할당되지 않았습니다.", dropped.itemData);
            return;
        }

        GameObject obj = Instantiate(
            dropped.itemData.worldPrefab,
            spawnPoint.position,
            Quaternion.identity
        );

        station.AddItem(obj);
    }
}
