using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Put this on the RecipeWindow (the Scroll View).
// Every time the window opens it rebuilds the list from the recipes inside MixManager,
// one row each, like:  Water = 2 Hydrogen, 1 Oxygen
public class RecipeListUI : MonoBehaviour
{
    public MixManager mixManager;
    public RectTransform content;   // the "Content" object inside the Scroll View
    public TMP_Text rowTemplate;    // a TMP text inside Content, turned OFF, used as the row design

    void OnEnable()
    {
        Build();
    }

    public void Build()
    {
        if (mixManager == null || content == null || rowTemplate == null)
        {
            Debug.LogWarning("[RecipeListUI] Drag in the MixManager, Content and RowTemplate first.");
            return;
        }

        // remove old rows (everything in Content except the template)
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            GameObject child = content.GetChild(i).gameObject;
            if (child == rowTemplate.gameObject) continue;
            Destroy(child);
        }

        // sort A to Z
        List<MixRecipe> sorted = new List<MixRecipe>(mixManager.recipes);
        sorted.RemoveAll(r => r == null);
        sorted.Sort((a, b) => string.Compare(a.resultName, b.resultName, System.StringComparison.OrdinalIgnoreCase));

        foreach (MixRecipe recipe in sorted)
        {
            TMP_Text row = Instantiate(rowTemplate, content);
            row.gameObject.SetActive(true);
            row.text = "<b>" + recipe.resultName + "</b> = " + Format(recipe);
        }
    }

    string Format(MixRecipe recipe)
    {
        List<string> parts = new List<string>();
        foreach (MixRecipe.Ingredient ing in recipe.ingredients)
        {
            if (ing != null) parts.Add(ing.amount + " " + ing.elementName);
        }
        return string.Join(", ", parts);
    }
}