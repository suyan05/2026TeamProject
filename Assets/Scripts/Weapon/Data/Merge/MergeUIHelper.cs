using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MergeUIHelper : MonoBehaviour
{
    public Inventory inventory;          // 인벤토리
    public List<MergeRecipe> recipes;    // 전체 조합식

    public List<MergeRecipe> GetAvailableRecipes()
    {
        if (inventory == null || recipes == null)
            return new List<MergeRecipe>();

        List<int> ownedIDs = inventory.items
            .Select(i => i.itemID)
            .ToList();

        List<MergeRecipe> available = new List<MergeRecipe>();

        foreach (var recipe in recipes)
        {
            if (CanMakeRecipe(recipe, ownedIDs))
                available.Add(recipe);
        }

        return available;
    }

    private bool CanMakeRecipe(MergeRecipe recipe, List<int> ownedIDs)
    {
        if (recipe == null || recipe.ingredientIDs == null)
            return false;

        List<int> ownedCopy = new List<int>(ownedIDs);

        foreach (int id in recipe.ingredientIDs)
        {
            if (!ownedCopy.Remove(id))
                return false;
        }

        return true;
    }
}
