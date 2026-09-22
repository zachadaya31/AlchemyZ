using System.Collections.Generic;
using UnityEngine;

public class MixManager : MonoBehaviour
{
    [Header("References")]
    public ElementSpawner elementSpawner;

    [Header("Water Recipe (H2O)")]
    public GameObject waterPrefab;

    public void OnMixPressed()
    {
        Dictionary<string, int> counts = ElementListUI.Instance.GetCounts();

        // recipe check: exactly 2 Hydrogen and 1 Oxygen, nothing else
        bool isWater =
            counts.Count == 2 &&
            counts.ContainsKey("Hydrogen") && counts["Hydrogen"] == 2 &&
            counts.ContainsKey("Oxygen") && counts["Oxygen"] == 1;

        if (isWater)
        {
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

        // spawn the result where the first element currently is
        Vector3 spawnPos = spawned.Count > 0 ? spawned[0].transform.position : Vector3.zero;

        Instantiate(resultPrefab, spawnPos, Quaternion.identity);

        // clear out the ingredient elements and reset the panel
        elementSpawner.ClearAllElements();
    }
}