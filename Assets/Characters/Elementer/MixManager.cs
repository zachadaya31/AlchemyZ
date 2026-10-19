using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class MixManager : MonoBehaviour
{
    [Header("References")]
    public ElementSpawner elementSpawner;

    [Header("Recipes")]
    [Tooltip("Drag all MixRecipe assets here (or use the context menu 'Load All Recipes In Project').")]
    public List<MixRecipe> recipes = new List<MixRecipe>();

    [Header("Instructions and Warning GameObject")]
    public GameObject instructions;
    public TextMeshProUGUI instructionsText;
    public GameObject warning;

    [Header("Instructions Text")]
    public string instructionsBefore;
    public string instructionsAfter;


    // MissionDirector can subscribe to these
    public event Action<MixRecipe> OnMixSucceeded;
    public event Action OnMixFailed;
    public event Action<MixRecipe, GameObject> OnResultTapped;

    // tracks spawned results so we can detect taps on them
    private class SpawnedResult
    {
        public GameObject obj;
        public MixRecipe recipe;
    }
    private readonly List<SpawnedResult> spawnedResults = new List<SpawnedResult>();

    void Update()
    {
#if UNITY_EDITOR
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 screenPos = Mouse.current.position.ReadValue();

            bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            if (overUI) return;

            CheckResultTap(screenPos);
        }
#else
        if (Touchscreen.current != null && Touchscreen.current.touches.Count > 0)
        {
            var touch = Touchscreen.current.primaryTouch;

            if (touch.press.wasPressedThisFrame)
            {
                Vector2 screenPos = touch.position.ReadValue();
                int touchId = touch.touchId.ReadValue();

                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touchId))
                    return;

                CheckResultTap(screenPos);
            }
        }
#endif
    }
    public bool IsOverResult(Vector2 screenPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        foreach (RaycastHit hit in Physics.RaycastAll(ray))
        {
            foreach (SpawnedResult result in spawnedResults)
            {
                if (result.obj == null) continue;
                Transform t = result.obj.transform;
                if (hit.collider.transform == t || hit.collider.transform.IsChildOf(t))
                    return true;
            }
        }
        return false;
    }
    private void CheckResultTap(Vector2 screenPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            foreach (SpawnedResult result in spawnedResults)
            {
                if (result.obj == null) continue;

                Transform t = result.obj.transform;
                if (hit.collider.transform == t || hit.collider.transform.IsChildOf(t))
                {
                    Debug.Log("Result tapped: " + result.obj.name);
                    Mission1.Instance.nextScene();
                    OnResultTapped?.Invoke(result.recipe, result.obj);
                    return;
                }
            }
        }
    }

    public void OnMixPressed()
    {
        // Normalise names so "hydrogen" and "Hydrogen" both work
        Dictionary<string, int> counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var kv in ElementListUI.Instance.GetCounts())
        {
            if (kv.Value <= 0) continue;
            counts[kv.Key.Trim()] = kv.Value;
        }

        MixRecipe match = FindRecipe(counts);

        if (match != null)
        {
            instructionsText.SetText("Tap to grab the result.");

            if (warning != null)
            {
                warning.SetActive(false);
            } else
            {
                
            }
            Debug.Log($"Mix success: {match.resultName} formed!");
            SpawnResult(match);
            OnMixSucceeded?.Invoke(match);
        }
        else
        {
            warning.SetActive(true);
            Debug.Log("Mix failed: no matching recipe.");
            OnMixFailed?.Invoke();
        }
    }

    private MixRecipe FindRecipe(Dictionary<string, int> counts)
    {
        foreach (MixRecipe recipe in recipes)
        {
            if (recipe != null && recipe.Matches(counts))
                return recipe;
        }
        return null;
    }

    private void SpawnResult(MixRecipe recipe)
    {
        if (recipe.resultPrefab == null)
        {
            Debug.LogError($"Recipe '{recipe.name}' has no result prefab!", recipe);
            return;
        }

        List<GameObject> spawned = elementSpawner.GetSpawnedElements();
        Vector3 spawnPos = spawned.Count > 0 ? spawned[0].transform.position : Vector3.zero;

        GameObject obj = Instantiate(recipe.resultPrefab, spawnPos, Quaternion.identity);
        spawnedResults.Add(new SpawnedResult { obj = obj, recipe = recipe });

        elementSpawner.ClearAllElements();
    }

#if UNITY_EDITOR
    // Handy: right-click the MixManager component > "Load All Recipes In Project"
    [ContextMenu("Load All Recipes In Project")]
    private void LoadAllRecipes()
    {
        recipes.Clear();
        foreach (string guid in UnityEditor.AssetDatabase.FindAssets("t:MixRecipe"))
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            recipes.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<MixRecipe>(path));
        }
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"Loaded {recipes.Count} recipes.");
    }

    // Warns about two recipes with identical ingredients
    private void OnValidate()
    {
        for (int i = 0; i < recipes.Count; i++)
        for (int j = i + 1; j < recipes.Count; j++)
        {
            if (recipes[i] == null || recipes[j] == null) continue;
            if (SameIngredients(recipes[i], recipes[j]))
                Debug.LogWarning($"Duplicate recipes: '{recipes[i].name}' and '{recipes[j].name}' have the same ingredients.", this);
        }
    }

    private static bool SameIngredients(MixRecipe a, MixRecipe b)
    {
        var ra = a.GetRequired();
        return ra.Count > 0 && a.Matches(b.GetRequired());
    }
#endif

    void Start()
        {
            instructions.SetActive(true);
            instructionsText.SetText(instructionsBefore);
        }
}