using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// One mixture. Create via: Right-click in Project > Create > Alchemyz > Mix Recipe
/// </summary>
[CreateAssetMenu(fileName = "NewMixRecipe", menuName = "Alchemyz/Mix Recipe")]
public class MixRecipe : ScriptableObject
{
    [Serializable]
    public class Ingredient
    {
        [Tooltip("Must match the element name used by ElementListUI, e.g. Hydrogen")]
        public string elementName;
        [Min(1)] public int amount = 1;
    }

    [Header("Recipe")]
    public string resultName = "Water";
    public List<Ingredient> ingredients = new List<Ingredient>();

    [Header("Result")]
    public GameObject resultPrefab;
    [TextArea] public string successMessage = "Tap to grab the result.";

    [Header("Mission")]
    [Tooltip("If true, tapping the result finishes the Elementer scene (current behaviour).")]
    public bool completesMission = true;

    /// <summary>Required amounts per element (case-insensitive, duplicates summed).</summary>
    public Dictionary<string, int> GetRequired()
    {
        var required = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var ing in ingredients)
        {
            if (ing == null || string.IsNullOrWhiteSpace(ing.elementName)) continue;
            string key = ing.elementName.Trim();
            required[key] = (required.TryGetValue(key, out int existing) ? existing : 0) + ing.amount;
        }
        return required;
    }

    /// <summary>True if the given element counts are exactly this recipe.</summary>
    public bool Matches(Dictionary<string, int> counts)
    {
        var required = GetRequired();
        if (required.Count == 0 || required.Count != counts.Count) return false;

        foreach (var kv in required)
        {
            if (!counts.TryGetValue(kv.Key, out int have) || have != kv.Value)
                return false;
        }
        return true;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (resultPrefab == null)
            Debug.LogWarning($"[MixRecipe] '{name}' has no result prefab assigned.", this);
    }
#endif
}
