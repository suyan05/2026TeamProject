// ---------------------------------------------------------
// DropZone
// - 드래그한 UI 아이템을 합성대에 드롭하면
//   MergeStation에 아이템을 추가함
// ---------------------------------------------------------
using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    public MergeStation station;

    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;

        if (dropped != null)
        {
            station.AddItem(dropped);
        }
    }
}
