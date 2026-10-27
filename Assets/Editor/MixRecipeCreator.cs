using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Put this file inside a folder called "Editor" (e.g. Assets/Editor/MixRecipeCreator.cs).
//
// Menu: AlchemyZ > Create All Mix Recipes
// For every item in the table below that has a prefab in Assets/Resources/Mixtures it will:
//   - create a MixRecipe asset in Assets/Resources/Mixtures/Recipes (or update it if it already exists)
//   - fill in the result name, the ingredients and the result prefab
// Then it adds every MixRecipe in the project to the MixManager in the open scene.
// Safe to run again: it updates recipes, it never makes duplicates.
public static class MixRecipeCreator
{
    const string MixturesFolder = "Assets/Resources/Mixtures";
    const string RecipesFolder = "Assets/Resources/Mixtures/Recipes";
    const string ElementsFolder = "Assets/Resources/Elements";

    // Format: Name;Other name|Symbol:count,Symbol:count
    // The first name is the result name. The names are matched to the prefab file names
    // (capitals, spaces, dashes, underscores and brackets are ignored).
    static readonly string[] rows =
    {
        "Water|H:2,O:1",
        "Hydrogen Peroxide;Hydrogen Peroxide(Clone)|H:2,O:2",
        "Carbon Dioxide|C:1,O:2",
        "Carbon Monoxide|C:1,O:1",
        "Methane|C:1,H:4",
        "Ethane|C:2,H:6",
        "Propane|C:3,H:8",
        "Butane|C:4,H:10",
        "Ammonia|N:1,H:3",
        "Nitric Oxide|N:1,O:1",
        "Nitrogen Dioxide|N:1,O:2",
        "Nitrous Oxide|N:2,O:1",
        "Ozone|O:3",
        "Hydrogen Sulfide|H:2,S:1",
        "Sulfur Dioxide;Sulfur Oxide|S:1,O:2",
        "Sulfur Trioxide|S:1,O:3",
        "Silicon Dioxide|Si:1,O:2",
        "Glucose|C:6,H:12,O:6",
        "Ethanol|C:2,H:6,O:1",
        "Acetic Acid|C:2,H:4,O:2",
        "Hydrochloric Acid;Hydrocloric Acid|H:1,Cl:1",
        "Hydrofluoric Acid|H:1,F:1",
        "Sulfuric Acid|H:2,S:1,O:4",
        "Nitric Acid|H:1,N:1,O:3",
        "Carbonic Acid|H:2,C:1,O:3",
        "Phosphoric Acid|H:3,P:1,O:4",
        "Sodium Hydroxide|Na:1,O:1,H:1",
        "Potassium Hydroxide|K:1,O:1,H:1",
        "Calcium Hydroxide|Ca:1,O:2,H:2",
        "Magnesium Hydroxide|Mg:1,O:2,H:2",
        "Sodium Chloride;Table Salt;Salt|Na:1,Cl:1",
        "Potassium Chloride|K:1,Cl:1",
        "Magnesium Chloride|Mg:1,Cl:2",
        "Calcium Chloride|Ca:1,Cl:2",
        "Silver Chloride|Ag:1,Cl:1",
        "Ammonium Chloride|N:1,H:4,Cl:1",
        "Sodium Bicarbonate;Baking Soda;Sodium Bicarbonated|Na:1,H:1,C:1,O:3",
        "Sodium Carbonate;Washing Soda|Na:2,C:1,O:3",
        "Calcium Carbonate;Limestone|Ca:1,C:1,O:3",
        "Copper Sulfate;Copper(II) Sulfate|Cu:1,S:1,O:4",
        "Sodium Nitrate|Na:1,N:1,O:3",
        "Potassium Nitrate;Saltpeter|K:1,N:1,O:3",
        "Magnesium Oxide|Mg:1,O:1",
        "Calcium Oxide;Quicklime|Ca:1,O:1",
        "Zinc Oxide|Zn:1,O:1",
        "Copper(II) Oxide;Copper Oxide|Cu:1,O:1",
        "Iron(II) Oxide;Iron Oxide|Fe:1,O:1",
        "Iron(III) Oxide;Rust|Fe:2,O:3",
        "Aluminum Oxide;Aluminium Oxide|Al:2,O:3",
        "Sodium Oxide|Na:2,O:1"
    };

    // symbol -> element prefab names to look for in Assets/Resources/Elements (first one found is used)
    static readonly Dictionary<string, string[]> symbolNames = new Dictionary<string, string[]>
    {
        { "H",  new[] { "Hydrogen" } },
        { "C",  new[] { "Carbon" } },
        { "N",  new[] { "Nitrogen" } },
        { "O",  new[] { "Oxygen" } },
        { "F",  new[] { "Fluorine" } },
        { "Na", new[] { "Sodium" } },
        { "Mg", new[] { "Magnesium" } },
        { "Al", new[] { "Aluminum", "Aluminium" } },
        { "Si", new[] { "Silicon" } },
        { "P",  new[] { "Phosphorus" } },
        { "S",  new[] { "Sulfur", "Sulphur" } },
        { "Cl", new[] { "Chlorine" } },
        { "K",  new[] { "Potassium" } },
        { "Ca", new[] { "Calcium" } },
        { "Fe", new[] { "Iron" } },
        { "Cu", new[] { "Copper" } },
        { "Zn", new[] { "Zinc" } },
        { "Ag", new[] { "Silver" } }
    };

    static string Normalize(string s)
    {
        return s.Trim().ToLowerInvariant()
            .Replace(" ", "").Replace("-", "").Replace("_", "")
            .Replace("(", "").Replace(")", "");
    }

    static string FindElementName(string symbol, Dictionary<string, string> elementPrefabs)
    {
        if (!symbolNames.TryGetValue(symbol, out string[] candidates)) return null;
        foreach (string c in candidates)
        {
            if (elementPrefabs.TryGetValue(Normalize(c), out string actual)) return actual;
        }
        return null;
    }

    [MenuItem("AlchemyZ/Create All Mix Recipes")]
    static void Run()
    {
        // element prefabs: normalized name -> real file name
        var elementPrefabs = new Dictionary<string, string>();
        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { ElementsFolder }))
        {
            string n = Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(guid));
            elementPrefabs[Normalize(n)] = n;
        }
        if (elementPrefabs.Count == 0)
        {
            Debug.LogError("[AlchemyZ] No element prefabs found in " + ElementsFolder + ". Check the folder name.");
            return;
        }

        // mixture prefabs: normalized name -> asset path
        var mixturePrefabs = new Dictionary<string, string>();
        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { MixturesFolder }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            mixturePrefabs[Normalize(Path.GetFileNameWithoutExtension(path))] = path;
        }

        if (!AssetDatabase.IsValidFolder(RecipesFolder))
            AssetDatabase.CreateFolder(MixturesFolder, "Recipes");

        int created = 0, updated = 0;
        var skipped = new List<string>();

        foreach (string row in rows)
        {
            string[] parts = row.Split('|');
            string[] names = parts[0].Split(';');
            string resultName = names[0];

            // find the prefab for this item
            string prefabPath = null;
            foreach (string n in names)
            {
                if (mixturePrefabs.TryGetValue(Normalize(n), out string p))
                {
                    prefabPath = p;
                    break;
                }
            }
            if (prefabPath == null)
            {
                skipped.Add(resultName + " (no prefab)");
                continue;
            }

            // build the ingredient list
            var ingredients = new List<MixRecipe.Ingredient>();
            bool ok = true;
            foreach (string part in parts[1].Split(','))
            {
                string[] sc = part.Split(':');
                string symbol = sc[0].Trim();
                int amount = int.Parse(sc[1]);

                string elementName = FindElementName(symbol, elementPrefabs);
                if (elementName == null)
                {
                    Debug.LogError("[AlchemyZ] " + resultName + ": no element prefab found for '" + symbol + "' in " + ElementsFolder);
                    ok = false;
                    break;
                }
                ingredients.Add(new MixRecipe.Ingredient { elementName = elementName, amount = amount });
            }
            if (!ok)
            {
                skipped.Add(resultName + " (missing element prefab)");
                continue;
            }

            // create or update the recipe asset
            string assetPath = RecipesFolder + "/" + resultName.Replace(" ", "") + "Recipe.asset";
            MixRecipe recipe = AssetDatabase.LoadAssetAtPath<MixRecipe>(assetPath);
            bool isNew = recipe == null;
            if (isNew) recipe = ScriptableObject.CreateInstance<MixRecipe>();

            recipe.resultName = resultName;
            recipe.ingredients = ingredients;
            recipe.resultPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (isNew)
            {
                AssetDatabase.CreateAsset(recipe, assetPath);
                created++;
            }
            else
            {
                EditorUtility.SetDirty(recipe);
                updated++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // put every recipe into the MixManager of the open scene
        string managerMessage;
        MixManager mm = Object.FindFirstObjectByType<MixManager>();
        if (mm != null)
        {
            var all = new List<MixRecipe>();
            foreach (string guid in AssetDatabase.FindAssets("t:MixRecipe"))
            {
                MixRecipe r = AssetDatabase.LoadAssetAtPath<MixRecipe>(AssetDatabase.GUIDToAssetPath(guid));
                if (r != null) all.Add(r);
            }

            SerializedObject so = new SerializedObject(mm);
            SerializedProperty list = so.FindProperty("recipes");
            list.arraySize = all.Count;
            for (int i = 0; i < all.Count; i++)
                list.GetArrayElementAtIndex(i).objectReferenceValue = all[i];
            so.ApplyModifiedProperties();
            EditorSceneManager.MarkSceneDirty(mm.gameObject.scene);

            managerMessage = "MixManager now has " + all.Count + " recipes. SAVE THE SCENE (Ctrl+S).";
        }
        else
        {
            managerMessage = "No MixManager in the open scene. Open the Elementer scene, right-click the MixManager component and pick 'Load All Recipes In Project'.";
        }

        Debug.Log("[AlchemyZ] Recipes created: " + created + " | updated: " + updated
            + " | skipped: " + skipped.Count + (skipped.Count > 0 ? " -> " + string.Join(", ", skipped) : ""));
        Debug.Log("[AlchemyZ] " + managerMessage);
    }
}
