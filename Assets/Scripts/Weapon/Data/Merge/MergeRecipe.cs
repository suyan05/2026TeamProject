using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewMergeRecipe", menuName = "Merge/Recipe")]
public class MergeRecipe : ScriptableObject
{
    public List<int> ingredientIDs = new List<int>();
    public int resultItemID;
}
