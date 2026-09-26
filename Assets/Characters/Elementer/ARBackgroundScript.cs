using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARBackgroundToggle : MonoBehaviour
{
    [SerializeField] private ARCameraBackground arBackground;

    public void SetBackgroundVisible(bool isVisible)
    {
        if (arBackground != null)
            arBackground.enabled = isVisible;
    }
}