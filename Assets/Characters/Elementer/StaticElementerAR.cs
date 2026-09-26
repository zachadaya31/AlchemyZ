using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class StaticElementerAR : MonoBehaviour
{
    public static StaticElementerAR Instance { get; private set; }

    [SerializeField] private Camera arCamera;
    [SerializeField] private ARSession arSession; // drag the AR Session object here

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetARCameraActive(bool isActive)
    {
        if (arCamera != null)
            arCamera.gameObject.SetActive(isActive);
    }
}