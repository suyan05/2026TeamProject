using UnityEngine;
using UnityEngine.UI;

public class MergeSlotUI : MonoBehaviour
{
    public ItemData item;
    public Image icon;

    public void SetItem(ItemData newItem)
    {
        item = newItem;
        icon.sprite = newItem.icon;
        icon.enabled = true;
    }
}
