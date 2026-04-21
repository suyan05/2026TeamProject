// ---------------------------------------------------------
// MergeRecipe
// - Inspector에서 직접 조합식을 만들 수 있는 ScriptableObject
// - ingredientIDs: 재료 아이템들의 ID 목록 (3~5개)
// - resultItemID: 결과 아이템 ID
// ---------------------------------------------------------
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewMergeRecipe", menuName = "Merge/Recipe")]
public class MergeRecipe : ScriptableObject
{
    [Header("재료 아이템 ID 목록 (3~5개)")]
    public List<int> ingredientIDs = new List<int>();

    [Header("결과 아이템 ID")]
    public int resultItemID;
}
