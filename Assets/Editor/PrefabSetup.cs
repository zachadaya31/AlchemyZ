using UnityEngine;
using UnityEditor;

public class PrefabSetup
{
    [MenuItem("Tools/Setup Selected Prefabs")]
    static void Setup()
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            GameObject root = PrefabUtility.LoadPrefabContents(path);

            if (root.GetComponent<Collider>() == null)
            {
                root.AddComponent<SphereCollider>();
            }

            if (root.GetComponent<ElementFloat>() == null)
            {
                root.AddComponent<ElementFloat>();
            }

            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
        }
    }
}