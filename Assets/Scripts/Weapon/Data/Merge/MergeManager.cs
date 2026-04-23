using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MergeManager : MonoBehaviour
{
    public MergeStationUI station;
    public Inventory inventory;

    // 기존 구조 유지: MergeManager 안에 레시피 리스트 보관
    public List<MergeRecipe> recipes = new List<MergeRecipe>();

    private void Awake()
    {
        if (station == null) station = FindObjectOfType<MergeStationUI>();
        if (inventory == null) inventory = FindObjectOfType<Inventory>();
    }

    public void TryMerge()
    {
        List<ItemInstance> instances = station.slots
            .Where(s => s.itemInstance != null)
            .Select(s => s.itemInstance)
            .ToList();

        if (instances.Count == 0)
        {
            Debug.Log("머지할 아이템이 없습니다.");
            return;
        }

        ItemData[] inputData = instances.Select(i => i.data).ToArray();

        MergeRecipe recipe = FindMatchingRecipe(inputData);

        if (recipe == null)
        {
            Debug.Log("일치하는 머지 레시피가 없습니다.");
            return;
        }

        bool added = inventory.TryAddItem(recipe.result);

        if (added)
        {
            Debug.Log("머지 성공!");

            station.ClearAll();
            FindObjectOfType<InventoryUI>().RefreshItems();
        }
        else
        {
            Debug.Log("인벤토리가 가득 찼습니다.");
        }
    }

    MergeRecipe FindMatchingRecipe(ItemData[] input)
    {
        foreach (var recipe in recipes)
        {
            if (recipe.ingredients.Length != input.Length)
                continue;

            if (recipe.ingredients.OrderBy(i => i.name)
                .SequenceEqual(input.OrderBy(i => i.name)))
            {
                return recipe;
            }
        }
        return null;
    }
}
