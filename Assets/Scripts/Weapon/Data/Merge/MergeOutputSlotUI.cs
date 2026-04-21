using UnityEngine;
using UnityEngine.UI;

public class MergeOutputSlotUI : MonoBehaviour
{
    public Image icon;

    public void SetResult(ItemData item)
    {
        icon.sprite = item.icon;
        icon.enabled = true;
    }

    public void Clear()
    {
        icon.enabled = false;
    }
}
