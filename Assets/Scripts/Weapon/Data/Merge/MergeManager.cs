using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MergeManager : MonoBehaviour
{
    public List<MergeRecipe> recipes;
    public ItemDatabase itemDB;
    public Inventory inventory;

    public MergeStationUI station;
    public MergeOutputSlotUI outputSlot;

    public void TryMerge()
    {
        List<ItemData> selected = new List<ItemData>();

        foreach (var slot in station.slotList)
        {
            MergeSlotUI s = slot.GetComponent<MergeSlotUI>();
            if (s.item != null)
                selected.Add(s.item);
        }

        if (selected.Count == 0)
            return;

        List<int> ids = selected.Select(i => i.itemID).ToList();

        foreach (var recipe in recipes)
        {
            if (IsMatch(recipe, ids))
            {
                foreach (var item in selected)
                    inventory.RemoveItem(item);

                ItemData result = itemDB.GetItemByID(recipe.resultItemID);
                inventory.TryAddItem(result);

                outputSlot.SetResult(result);

                station.ClearSlots();
                return;
            }
        }

        Debug.Log("조합 실패");
    }

    bool IsMatch(MergeRecipe recipe, List<int> ids)
    {
        if (recipe.ingredientIDs.Count != ids.Count)
            return false;

        return recipe.ingredientIDs.OrderBy(i => i)
            .SequenceEqual(ids.OrderBy(i => i));
    }
}
