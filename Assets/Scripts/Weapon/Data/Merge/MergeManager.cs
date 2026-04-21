using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MergeManager : MonoBehaviour
{
    public Transform dropPoint;       // 결과 아이템 생성 위치
    public List<MergeRecipe> recipes; // 등록된 조합식
    public ItemDatabase itemDB;       // 아이템 DB

    public void TryMerge(List<ItemData> selectedItems)
    {
        if (selectedItems == null || selectedItems.Count == 0)
            return;

        List<int> selectedIDs = selectedItems.Select(i => i.itemID).ToList();

        foreach (var recipe in recipes)
        {
            if (IsMatch(recipe, selectedIDs))
            {
                SpawnResult(recipe.resultItemID);
                return;
            }
        }

        Debug.Log("조합 실패: 해당 조합식 없음");
    }

    bool IsMatch(MergeRecipe recipe, List<int> selectedIDs)
    {
        if (recipe.ingredientIDs.Count != selectedIDs.Count)
            return false;

        return recipe.ingredientIDs.OrderBy(i => i)
            .SequenceEqual(selectedIDs.OrderBy(i => i));
    }

    void SpawnResult(int resultID)
    {
        ItemData result = itemDB.GetItemByID(resultID);

        if (result == null || result.worldPrefab == null)
            return;

        Instantiate(result.worldPrefab, dropPoint.position, Quaternion.identity);
    }
}
