// Inspector에서 직접 조합식 생성 (ID 기반)
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewMergeRecipe", menuName = "Merge/Recipe")]
public class MergeRecipe : ScriptableObject
{
    public List<int> ingredientIDs = new List<int>(); // 재료 ID 리스트
    public int resultItemID;                          // 결과 ID
}