using UnityEngine;
using UnityEngine.SceneManagement;

public class elementerScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        SceneLoader.cleanScenes("Mission1");
        Mission1.Instance.nextScene();

    }
}
