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

    // NEW: maps each spawned GameObject to its element name
    private Dictionary<GameObject, string> elementNames = new Dictionary<GameObject, string>();

    void Update()
    {
    #if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            if (overUI) return;

            GameObject touchedElement = GetElementUnderPosition(Input.mousePosition);

            if (touchedElement != null)
                StartHold(touchedElement);
            else
                spawnElement(Input.mousePosition);
        }

        if (Input.GetMouseButtonUp(0))
            CancelHold();

    #else
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                return;

            if (touch.phase == TouchPhase.Began)
            {
                GameObject touchedElement = GetElementUnderPosition(touch.position);

                if (touchedElement != null)
                    StartHold(touchedElement);
                else
                    spawnElement(touch.position);
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                CancelHold();
            }
        }
    #endif
    }

    // checks if the given screen position is on top of an already-spawned element
    private GameObject GetElementUnderPosition(Vector2 screenPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (spawnedElements.Contains(hit.collider.gameObject))
                return hit.collider.gameObject;
        }
        return null;
    }

    [Header("Delete Settings")]
    public float holdDuration = 1f; // how long to hold before element deletes

    private Coroutine holdCoroutine;

    private void StartHold(GameObject element)
    {
        holdCoroutine = StartCoroutine(HoldToDelete(element));
    }

    private void CancelHold()
    {
        if (holdCoroutine != null)
        {
            StopCoroutine(holdCoroutine);
            holdCoroutine = null;
        }
    }

    private System.Collections.IEnumerator HoldToDelete(GameObject element)
    {
        yield return new WaitForSeconds(holdDuration);

        if (element != null)
        {
            if (elementNames.TryGetValue(element, out string name))
            {
                ElementListUI.Instance.RemoveRow(name);
                elementNames.Remove(element);
            }

            spawnedElements.Remove(element);
            Destroy(element);
        }

        holdCoroutine = null;
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
            elementNames[spawned] = ElementSelecter.elementSelected;

            // NEW: tell the panel to add a row
            ElementListUI.Instance.AddRow(ElementSelecter.elementSelected);

            
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
        elementNames.Clear();

        ElementListUI.Instance.ClearAll();
    }

    public void openElementLibrary()
    {
        valorantEffect.SetActive(true);
        elementLibrary.SetActive(true);
    }
    
    public List<GameObject> GetSpawnedElements()
    {
        return spawnedElements;
    }
}