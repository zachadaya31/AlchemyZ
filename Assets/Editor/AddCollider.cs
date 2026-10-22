using UnityEditor;
using UnityEngine;

public class AddBoxColliders
{
    [MenuItem("Tools/Add Element Box Colliders")]
    static void AddAll()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Resources/Elements" });
        int added = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = PrefabUtility.LoadPrefabContents(path);

            // skip any prefab that already has a collider
            if (prefab.GetComponent<Collider>() == null)
            {
                Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();

                if (renderers.Length > 0)
                {
                    Bounds b = renderers[0].bounds;
                    foreach (Renderer r in renderers)
                    {
                        b.Encapsulate(r.bounds);
                    }

                    BoxCollider col = prefab.AddComponent<BoxCollider>();
                    col.center = prefab.transform.InverseTransformPoint(b.center);

                    Vector3 s = prefab.transform.lossyScale;
                    col.size = new Vector3(b.size.x / s.x, b.size.y / s.y, b.size.z / s.z);

                    PrefabUtility.SaveAsPrefabAsset(prefab, path);
                    added++;
                }
            }

            PrefabUtility.UnloadPrefabContents(prefab);
        }

        Debug.Log("Added box colliders to " + added + " prefabs");
    }
}