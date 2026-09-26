using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
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
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 screenPos = Mouse.current.position.ReadValue();

            bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            if (overUI) return;

            GameObject touchedElement = GetElementUnderPosition(screenPos);

            if (touchedElement != null)
                StartHold(touchedElement);
            else
                spawnElement(screenPos);
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame)
            CancelHold();

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

                GameObject touchedElement = GetElementUnderPosition(screenPos);

                if (touchedElement != null)
                    StartHold(touchedElement);
                else
                    spawnElement(screenPos);
            }
        }
#endif
    }

    private GameObject GetElementUnderPosition(Vector2 screenPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // walk up in case the collider is on a child object
            foreach (GameObject element in spawnedElements)
            {
                if (hit.collider.transform == element.transform || hit.collider.transform.IsChildOf(element.transform))
                    return element;
            }
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