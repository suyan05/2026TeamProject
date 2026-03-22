// 조합 처리 + 결과 아이템 바닥 드롭
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MergeManager : MonoBehaviour
{
    public Transform dropPoint;
    public List<MergeRecipe> recipes;
    public ItemDatabase itemDB;

    public void TryMerge(List<ItemData> selectedItems)
    {
        List<int> selectedIDs = selectedItems.Select(i => i.itemID).ToList();

        foreach (var recipe in recipes)
        {
            if (IsMatch(recipe, selectedIDs))
            {
                SpawnResult(recipe.resultItemID);
                return;
            }
        }
    }

    bool IsMatch(MergeRecipe recipe, List<int> selectedIDs)
    {
        if (recipe.ingredientIDs.Count != selectedIDs.Count)
            return false;

        return recipe.ingredientIDs.All(id => selectedIDs.Contains(id));
    }

    void SpawnResult(int resultID)
    {
        ItemData result = itemDB.GetItemByID(resultID);
        if (result == null || result.worldPrefab == null)
            return;

        Instantiate(result.worldPrefab, dropPoint.position, Quaternion.identity);
    }
}