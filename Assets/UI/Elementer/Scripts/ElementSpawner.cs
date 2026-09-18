using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ElementSpawner : MonoBehaviour
{
    
    public ARRaycastManager raycastManager;

    [Header("Element Library")]
    public GameObject valorantEffect;
    public GameObject elementLibrary;

    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    public void spawnElement() {
        GameObject elementToSpawn = Resources.Load<GameObject>("Elements/" + ElementSelecter.elementSelected + "Prefab");
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

        if (raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;

            Vector3 directionToCamera = Camera.main.transform.position - hitPose.position;
            directionToCamera.y = 0f;
            Quaternion faceCameraRotation = Quaternion.LookRotation(directionToCamera);

            Instantiate(elementToSpawn, hitPose.position, faceCameraRotation);
        }
        else {
            Debug.Log("No plane spotted");
        }
    }

    public void openElementLibrary() { 
        valorantEffect.SetActive(true);
        elementLibrary.SetActive(true);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
