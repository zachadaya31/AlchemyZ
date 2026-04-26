using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class SpawnOnFirstPlane : MonoBehaviour
{
    public ARPlaneManager planeManager;
    public GameObject objectPrefab;
    public Transform anchorElementer;

    private bool hasSpawned = false;
    private void Start()
    {
        planeManager.requestedDetectionMode = PlaneDetectionMode.Horizontal;
    }
    void Update()
    {
        if (!hasSpawned)
        {
            foreach (var plane in planeManager.trackables)
            {
                // First detected plane
                hasSpawned = true;

                Vector3 spawnPos = plane.transform.position;
                Instantiate(objectPrefab, spawnPos, objectPrefab.transform.rotation);

                Debug.Log("Spawned object on first detected plane (Unity 6)");
                break;
            }
        }
    }

}