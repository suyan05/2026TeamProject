using UnityEngine;

[CreateAssetMenu(menuName = "Merge/MergeRecipe")]
public class MergeRecipe : ScriptableObject
{
    public ItemData[] ingredients;
    public ItemData result;
}
