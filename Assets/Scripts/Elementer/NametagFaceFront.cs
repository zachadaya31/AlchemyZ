using UnityEngine;
using Unity.XR.CoreUtils;

public class NametagFaceFront : MonoBehaviour
{
    Camera arCam;

    void LateUpdate()
    {
        if (arCam == null)
        {
            XROrigin origin = FindAnyObjectByType<XROrigin>();
            if (origin != null && origin.Camera != null) arCam = origin.Camera;
            else arCam = Camera.main;
        }

        if (arCam != null)
        {
            transform.rotation = arCam.transform.rotation;
        }
    }
}