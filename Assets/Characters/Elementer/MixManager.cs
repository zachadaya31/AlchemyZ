using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Unity.XR.CoreUtils;

public class MixManager : MonoBehaviour
{

    [Header("Mission")]
    [Tooltip("Only this result moves the story on when tapped. Leave empty to let any result move it on.")]
    public string requiredResultName = "";

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

        private Camera arCam;

    private Camera GetCam()
    {
        if (arCam == null)
        {
            XROrigin origin = FindAnyObjectByType<XROrigin>();
            if (origin != null && origin.Camera != null) arCam = origin.Camera;
            else arCam = Camera.main;
        }
        return arCam;
    }

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
        if (Physics.Raycast(ray, out RaycastHit hit))
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

                    if (!IsWantedResult(result.recipe))
                    {
                        Debug.Log("Not the mixture this mission needs, ignoring.");
                        return;
                    }

                    if (TutorialManager.instance == null && Mission1.Instance != null) Mission1.Instance.nextScene();
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
            var flasher = warning.GetComponent<FlashingText>();
            if (flasher != null)
            {
                flasher.TriggerFlash();
            }
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
        Vector3 spawnPos = Vector3.zero;
        if (spawned.Count > 0)
        {
            ElementFloat floatScript = spawned[0].GetComponent<ElementFloat>();
            spawnPos = floatScript != null ? floatScript.GroundPosition : spawned[0].transform.position;
        }

        GameObject obj = Instantiate(recipe.resultPrefab, spawnPos, Quaternion.identity);
        SceneManager.MoveGameObjectToScene(obj, gameObject.scene); // NEW
        float multiplier = 1f;
        ElementScale scaleConfig = obj.GetComponent<ElementScale>();
        if (scaleConfig != null) multiplier = scaleConfig.sizeMultiplier;
        obj.transform.localScale = Vector3.one * elementSpawner.elementScale * multiplier;
        spawnedResults.Add(new SpawnedResult { obj = obj, recipe = recipe });

        elementSpawner.ClearElementsOnly();
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

    private bool IsWantedResult(MixRecipe recipe)
    {
        if (string.IsNullOrEmpty(requiredResultName)) return true;
        if (string.Equals(recipe.resultName, requiredResultName, StringComparison.OrdinalIgnoreCase)) return true;
        return recipe.resultPrefab != null &&
            string.Equals(recipe.resultPrefab.name, requiredResultName, StringComparison.OrdinalIgnoreCase);
    }
public GameObject GetResultUnderPosition(Vector2 screenPos)
{
    Ray ray = GetCam().ScreenPointToRay(screenPos);
    if (Physics.Raycast(ray, out RaycastHit hit))
    {
        foreach (SpawnedResult result in spawnedResults)
        {
            if (result.obj == null) continue;
            Transform t = result.obj.transform;
            if (hit.collider.transform == t || hit.collider.transform.IsChildOf(t))
                return result.obj;
        }
    }
    return null;
}

    public void RemoveResult(GameObject obj)
    {
        spawnedResults.RemoveAll(r => r.obj == obj);
    }

    public void ClearAllResults()
    {
        foreach (SpawnedResult r in spawnedResults)
        {
            if (r.obj != null) Destroy(r.obj);
        }
        spawnedResults.Clear();
    }

    
#endif

    void Start()
        {
            instructions.SetActive(true);
            instructionsText.SetText(instructionsBefore);
        }
}