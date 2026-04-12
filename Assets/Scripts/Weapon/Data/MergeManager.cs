using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MergeManager : MonoBehaviour
{
    [Header("결과 아이템이 생성될 위치")]
    public Transform dropPoint;

    [Header("등록된 모든 조합식")]
    public List<MergeRecipe> recipes;

    [Header("아이템 데이터베이스")]
    public ItemDatabase itemDB;

    private void Awake()
    {
        if (dropPoint == null)
            Debug.LogWarning("dropPoint가 설정되지 않았습니다.", this);
        if (itemDB == null)
            Debug.LogWarning("itemDB가 설정되지 않았습니다.", this);
        if (recipes == null || recipes.Count == 0)
            Debug.LogWarning("recipes가 비어 있습니다.", this);
    }

    public void TryMerge(List<ItemData> selectedItems)
    {
        if (selectedItems == null || selectedItems.Count == 0)
        {
            Debug.Log("선택된 아이템이 없습니다.");
            return;
        }

        List<int> selectedIDs = selectedItems.Select(i => i.ItemID).ToList();

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

        var sortedRecipe = recipe.ingredientIDs.OrderBy(id => id);
        var sortedSelected = selectedIDs.OrderBy(id => id);

        return sortedRecipe.SequenceEqual(sortedSelected);
    }

    void SpawnResult(int resultID)
    {
        if (itemDB == null)
        {
            Debug.LogError("itemDB가 설정되지 않았습니다.", this);
            return;
        }

        ItemData result = itemDB.GetItemByID(resultID);
        if (result == null || result.worldPrefab == null)
        {
            Debug.LogError("결과 아이템 또는 프리팹이 설정되지 않음", this);
            return;
        }

        if (dropPoint == null)
        {
            Debug.LogError("dropPoint가 설정되지 않았습니다.", this);
            return;
        }

        Instantiate(result.worldPrefab, dropPoint.position, Quaternion.identity);
    }
}
