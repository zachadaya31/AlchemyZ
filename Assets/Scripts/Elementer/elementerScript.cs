using UnityEngine;
using UnityEngine.SceneManagement;

public class elementerScript : MonoBehaviour
{
    private bool hasBeenClicked = false;

    void Start()
    {
        
    }
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        if (hasBeenClicked) return;
        hasBeenClicked = true;

        SceneLoader.cleanScenes("Mission1");
        Mission1.Instance.nextScene();
    }
}