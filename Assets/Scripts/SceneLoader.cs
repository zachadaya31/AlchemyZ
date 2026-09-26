using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public static void loadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void quitGame() {
        Application.Quit();
        Debug.Log("Application CLosed");
    }

    private static readonly string[] scenesToClean = { "AR_Test", "Elementer" };

    public static void cleanScenes(string mainScene)
    {
        foreach (string sceneName in scenesToClean)
        {
            Scene scene = SceneManager.GetSceneByName(sceneName);
            if (scene.IsValid() && scene.isLoaded)
                SceneManager.UnloadSceneAsync(scene);
        }
    }
}
