using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class MixManager : MonoBehaviour
{
    [Header("References")]
    public ElementSpawner elementSpawner;

    [Header("Water Recipe (H2O)")]
    public GameObject waterPrefab;

    // tracks spawned results so we can detect taps on them
    private List<GameObject> spawnedResults = new List<GameObject>();

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

    private void CheckResultTap(Vector2 screenPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            foreach (GameObject result in spawnedResults)
            {
                if (hit.collider.transform == result.transform || hit.collider.transform.IsChildOf(result.transform))
                {
                    OnResultTapped(result);
                    return;
                }
            }
        }
    }

    private void OnResultTapped(GameObject result)
    {
        Debug.Log("Result tapped: " + result.name);
        UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync("Elementer");
        Mission1.Instance.nextScene();
    }

    public void OnMixPressed()
    {
        Dictionary<string, int> counts = ElementListUI.Instance.GetCounts();

        bool isWater =
            counts.Count == 2 &&
            counts.ContainsKey("Hydrogen") && counts["Hydrogen"] == 2 &&
            counts.ContainsKey("Oxygen") && counts["Oxygen"] == 1;

        if (isWater)
        {
            Mission1.Instance.instructionsText.SetText("Tap to grab the water.");
            Debug.Log("Mix success: Water formed!");
            SpawnResult(waterPrefab);
        }
        else
        {
            Debug.Log("Mix failed: no matching recipe.");
        }
    }

    private void SpawnResult(GameObject resultPrefab)
    {
        List<GameObject> spawned = elementSpawner.GetSpawnedElements();
        Vector3 spawnPos = spawned.Count > 0 ? spawned[0].transform.position : Vector3.zero;

        GameObject result = Instantiate(resultPrefab, spawnPos, Quaternion.identity);
        spawnedResults.Add(result);

        elementSpawner.ClearAllElements();
    }
}