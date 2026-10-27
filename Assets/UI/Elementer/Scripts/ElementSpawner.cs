using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.XR.CoreUtils;
using UnityEngine.SceneManagement;

public class ElementSpawner : MonoBehaviour
{
    public ARRaycastManager raycastManager;
    public MixManager mixManager;

    [Header("Element Library")]
    public GameObject valorantEffect;
    public GameObject elementLibrary;

    [Header("Spawn Settings")]
    public float elementScale = 1f;

    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

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

    // tracks every element currently spawned
    private List<GameObject> spawnedElements = new List<GameObject>();

    // maps each spawned GameObject to its element name
    private Dictionary<GameObject, string> elementNames = new Dictionary<GameObject, string>();

    void Update()
    {
#if UNITY_EDITOR
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 screenPos = Mouse.current.position.ReadValue();

            bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            if (overUI) return;

            HandlePress(screenPos);
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame)
            CancelHold();
#else
        if (Touchscreen.current != null)
        {
            var touch = Touchscreen.current.primaryTouch;

            if (touch.press.wasPressedThisFrame)
            {
                Vector2 screenPos = touch.position.ReadValue();
                int touchId = touch.touchId.ReadValue();

                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touchId))
                    return;

                HandlePress(screenPos);
            }

            if (touch.press.wasReleasedThisFrame)
                CancelHold();
        }
#endif
    }

    // one place for what happens when the screen is pressed
    private void HandlePress(Vector2 screenPos)
    {
        // pressing a mixture result: start the hold-to-delete on it (no spawning)
        GameObject result = mixManager != null ? mixManager.GetResultUnderPosition(screenPos) : null;
        if (result != null)
        {
            StartHold(result);
            return;
        }

        GameObject touchedElement = GetElementUnderPosition(screenPos);

        if (touchedElement != null)
            StartHold(touchedElement);
        else
            spawnElement(screenPos);
    }

    private GameObject GetElementUnderPosition(Vector2 screenPos)
    {
        Ray ray = GetCam().ScreenPointToRay(screenPos);
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
    public float holdDuration = 1f; // how long to hold before it deletes

    private Coroutine holdCoroutine;

    private void StartHold(GameObject target)
    {
        CancelHold();
        holdCoroutine = StartCoroutine(HoldToDelete(target));
    }

    private void CancelHold()
    {
        if (holdCoroutine != null)
        {
            StopCoroutine(holdCoroutine);
            holdCoroutine = null;
        }
    }

    // works for both elements and mixture results
    private System.Collections.IEnumerator HoldToDelete(GameObject target)
    {
        yield return new WaitForSeconds(holdDuration);

        if (target != null)
        {
            // if it is an element, remove its row from the list panel
            if (elementNames.TryGetValue(target, out string name))
            {
                ElementListUI.Instance.RemoveRow(name);
                elementNames.Remove(target);
            }

            spawnedElements.Remove(target);

            // if it is a mixture result, forget it in MixManager
            if (mixManager != null) mixManager.RemoveResult(target);

            Destroy(target);
        }

        holdCoroutine = null;
    }

    public void spawnElement(Vector2 touchPosition)
    {
        GameObject elementToSpawn = Resources.Load<GameObject>("Elements/" + ElementSelecter.elementSelected);

        if (raycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;

            Vector3 directionToCamera = GetCam().transform.position - hitPose.position;
            directionToCamera.y = 0f;
            Quaternion faceCameraRotation = Quaternion.LookRotation(directionToCamera);

            GameObject spawned = Instantiate(elementToSpawn, hitPose.position, faceCameraRotation);
            SceneManager.MoveGameObjectToScene(spawned, gameObject.scene);

            // Check if the prefab has a custom scale modifier, default to 1.0f if missing
            float multiplier = 1.0f;
            ElementScale scaleConfig = spawned.GetComponent<ElementScale>();
            if (scaleConfig != null)
            {
                multiplier = scaleConfig.sizeMultiplier;
            }

            // Apply base scale multiplied by the specific prefab's factor
            spawned.transform.localScale = Vector3.one * elementScale * multiplier;

            spawnedElements.Add(spawned);
            elementNames[spawned] = ElementSelecter.elementSelected;

            // tell the panel to add a row
            ElementListUI.Instance.AddRow(ElementSelecter.elementSelected);
        }
        else
        {
            Debug.Log("No plane spotted");
        }
    }

    // The CLEAR button uses this: removes elements AND every mixture result
    public void ClearAllElements()
    {
        ClearElementsOnly();
        if (mixManager != null) mixManager.ClearAllResults();
    }

    // MixManager uses this after a successful mix: removes elements but KEEPS the new result
    public void ClearElementsOnly()
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