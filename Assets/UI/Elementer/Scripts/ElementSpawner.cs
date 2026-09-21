using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ElementSpawner : MonoBehaviour
{
    public ARRaycastManager raycastManager;

    [Header("Element Library")]
    public GameObject valorantEffect;
    public GameObject elementLibrary;

    [Header("Spawn Settings")]
    public float elementScale = 1f;

    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    // tracks every element currently spawned
    private List<GameObject> spawnedElements = new List<GameObject>();

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Click detected");

            bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            Debug.Log("Over UI: " + overUI);

            if (overUI)
                return;

            spawnElement(Input.mousePosition);
        }
#else
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                    return;

                spawnElement(touch.position);
            }
        }
#endif
    }

    public void spawnElement(Vector2 touchPosition)
    {
        GameObject elementToSpawn = Resources.Load<GameObject>("Elements/" + ElementSelecter.elementSelected + "Prefab");

        if (raycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;

            Vector3 directionToCamera = Camera.main.transform.position - hitPose.position;
            directionToCamera.y = 0f;
            Quaternion faceCameraRotation = Quaternion.LookRotation(directionToCamera);

            GameObject spawned = Instantiate(elementToSpawn, hitPose.position, faceCameraRotation);
            spawned.transform.localScale = Vector3.one * elementScale;

            spawnedElements.Add(spawned);
        }
        else
        {
            Debug.Log("No plane spotted");
        }
    }

    public void ClearAllElements()
    {
        foreach (GameObject element in spawnedElements)
        {
            if (element != null)
                Destroy(element);
        }

        spawnedElements.Clear();
    }

    public void openElementLibrary()
    {
        valorantEffect.SetActive(true);
        elementLibrary.SetActive(true);
    }
}