using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;

public class SpawnElementer : MonoBehaviour
{
    public ARPlaneManager planeManager;
    public GameObject elementer;

    private bool hasSpawned = false;

    void Update()
    {
        if (hasSpawned) return;

        if (planeManager.trackables.count > 0)
        {
            foreach (var plane in planeManager.trackables)
            {
                GameObject spawned = Instantiate(elementer, plane.transform.position, Quaternion.Euler(0f, 0f, -90f));
                SceneManager.MoveGameObjectToScene(spawned, gameObject.scene); // NEW
                elementer.transform.LookAt(Camera.main.transform);
                hasSpawned = true;
                break;
            }
        }
    }
}