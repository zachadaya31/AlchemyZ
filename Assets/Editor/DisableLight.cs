using UnityEditor;
using UnityEngine;

public class DisableLights
{
    [MenuItem("Tools/Disable Element Lights")]
    static void DisableAll()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Resources/Elements" });
        int changed = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = PrefabUtility.LoadPrefabContents(path);

            Light[] lights = prefab.GetComponentsInChildren<Light>(true);

            if (lights.Length > 0)
            {
                foreach (Light l in lights)
                {
                    l.enabled = false;
                }
                PrefabUtility.SaveAsPrefabAsset(prefab, path);
                changed++;
            }

            PrefabUtility.UnloadPrefabContents(prefab);
        }

        Debug.Log("Disabled lights in " + changed + " prefabs");
    }
}