using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class SpawnElementer : MonoBehaviour
{

    public ARRaycastManager raycastManager;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    public GameObject objectToSpawn;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame) {
            Vector2 touchPosition = Mouse.current.position.ReadValue();
            Debug.Log("Pressed at: "+touchPosition); 

            raycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon);
            Instantiate(objectToSpawn, hits[0].pose.position, hits[0].pose.rotation);
        }
    }
}
