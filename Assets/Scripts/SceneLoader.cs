using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {

    }

    public static void loadScene(string sceneName)
    {
        LoadingScreen.Load(sceneName);
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