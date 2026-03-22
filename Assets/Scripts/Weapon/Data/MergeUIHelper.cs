// 인벤토리 기반으로 가능한 조합식 자동 필터링
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MergeUIHelper : MonoBehaviour
{
    public Inventory inventory;
    public List<MergeRecipe> recipes;

    public List<MergeRecipe> GetAvailableRecipes()
    {
        List<int> ownedIDs = inventory.items.Select(i => i.itemID).ToList();
        List<MergeRecipe> available = new List<MergeRecipe>();

        foreach (var recipe in recipes)
        {
            bool canMake = recipe.ingredientIDs.All(id => ownedIDs.Contains(id));
            if (canMake) available.Add(recipe);
        }

        return available;
    }
}