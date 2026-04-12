// ---------------------------------------------------------
// MergeRecipeUI
// - UI에 "현재 만들 수 있는 조합식"을 자동으로 표시
// ---------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MergeRecipeUI : MonoBehaviour
{
    public MergeUIHelper helper;
    public ItemDatabase itemDB;
    public GameObject recipeSlotPrefab;
    public Transform content;

    void OnEnable()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        foreach (Transform child in content)
            Destroy(child.gameObject);

        List<MergeRecipe> available = helper.GetAvailableRecipes();

        foreach (var recipe in available)
        {
            GameObject slot = Instantiate(recipeSlotPrefab, content);
            var text = slot.GetComponentInChildren<Text>();

            if (text != null)
            {
                string ingNames = "";
                foreach (int id in recipe.ingredientIDs)
                    ingNames += itemDB.GetItemByID(id).itemName + " + ";

                ingNames = ingNames.TrimEnd(' ', '+');
                string resultName = itemDB.GetItemByID(recipe.resultItemID).itemName;

                text.text = $"{ingNames} = {resultName}";
            }
        }
    }
}
