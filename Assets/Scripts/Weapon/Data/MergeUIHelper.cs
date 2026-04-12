using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MergeUIHelper : MonoBehaviour
{
    public Inventory inventory;
    public List<MergeRecipe> recipes;

    //인벤토리에서 현재 만들 수 있는 조합식만 반환
    public List<MergeRecipe> GetAvailableRecipes()
    {
        if (inventory == null || recipes == null)
            return new List<MergeRecipe>();

        // 인벤토리의 아이템 ID 목록
        var ownedIDs = inventory.Items.Select(i => i.ItemID).ToList();
        var available = new List<MergeRecipe>();

        foreach (var recipe in recipes)
        {
            if (CanMakeRecipe(recipe, ownedIDs))
                available.Add(recipe);
        }
        return available;
    }

    
    //중복 재료까지 고려하여 조합 가능 여부 판단
    private bool CanMakeRecipe(MergeRecipe recipe, List<int> ownedIDs)
    {
        if (recipe == null || recipe.ingredientIDs == null)
            return false;

        var ownedCopy = new List<int>(ownedIDs);
        foreach (var id in recipe.ingredientIDs)
        {
            if (!ownedCopy.Remove(id))
                return false;
        }
        return true;
    }
}
