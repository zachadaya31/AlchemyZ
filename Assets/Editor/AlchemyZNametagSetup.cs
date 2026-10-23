using TMPro;
using UnityEditor;
using UnityEngine;

// MUST live in a folder named "Editor" (e.g. Assets/Editor/AlchemyZNametagSetup.cs)
//
// HOW TO USE:
// 1. Select ONE object that already has a working Nametag (the Nametag itself or its parent).
//    Use something OUTSIDE Assets/Resources/Elements (a scene clone in Play mode, or a template prefab).
// 2. Menu: AlchemyZ > Add Nametag + Tap Info To Element Prefabs
//
// Safe to run again and again: it REPLACES the Nametag on every element prefab each time.
// Tweak the two numbers below, then re-run.
public static class AlchemyZNametagSetup
{
    const string ElementsFolder = "Assets/Resources/Elements";

    // Make it bigger or smaller (1 = same size as your template, 1.5 = 50% bigger)
    const float SizeMultiplier = 3f;

    // Gap between the top of the model and the nametag, in meters
    const float HeightMargin = 0.001f;

    [MenuItem("AlchemyZ/Add Nametag + Tap Info To Element Prefabs")]
    static void Run()
    {
        GameObject sel = Selection.activeGameObject;
        if (sel == null)
        {
            Debug.LogError("[AlchemyZ] Select an object that has a Nametag first, then run this again.");
            return;
        }

        Transform template = sel.name == "Nametag" ? sel.transform : sel.transform.Find("Nametag");
        if (template == null)
        {
            Debug.LogError("[AlchemyZ] The selected object has no child called 'Nametag'.");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { ElementsFolder });
        int tagsAdded = 0, tapsAdded = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                // remove the old nametag so the model bounds are measured clean
                Transform old = root.transform.Find("Nametag");
                if (old != null) Object.DestroyImmediate(old.gameObject);

                // find the top of the model (in the prefab's local space)
                Vector3 center;
                float top;
                bool hasBounds = GetModelBounds(root, out center, out top);

                Vector3 rootScale = root.transform.lossyScale;

                GameObject tag = Object.Instantiate(template.gameObject, root.transform);
                tag.name = "Nametag";
                tag.transform.localRotation = Quaternion.identity;

                // same WORLD size on every element, no matter how big the model's scale is
                tag.transform.localScale = Div(template.lossyScale * SizeMultiplier, rootScale);

                if (hasBounds)
                    tag.transform.localPosition = new Vector3(center.x, top + HeightMargin / SafeY(rootScale), center.z);
                else
                    tag.transform.localPosition = template.localPosition;

                TMP_Text text = tag.GetComponentInChildren<TMP_Text>(true);
                if (text != null) text.text = root.name;
                tagsAdded++;

                if (root.GetComponent<ElementNametagTap>() == null)
                {
                    ElementNametagTap tap = root.AddComponent<ElementNametagTap>();
                    tap.elementName = root.name;
                    tapsAdded++;
                }

                if (root.GetComponentInChildren<Collider>(true) == null)
                    Debug.LogWarning("[AlchemyZ] No Collider on: " + path);

                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[AlchemyZ] Done. Nametags placed: " + tagsAdded + " | Tap components added: " + tapsAdded + " | Prefabs checked: " + guids.Length);
    }

    static bool GetModelBounds(GameObject root, out Vector3 center, out float top)
    {
        bool has = false;
        Bounds b = new Bounds();

        foreach (Renderer r in root.GetComponentsInChildren<Renderer>())
        {
            Bounds wb = r.bounds;
            for (int i = 0; i < 8; i++)
            {
                Vector3 corner = wb.center + new Vector3(
                    (i & 1) == 0 ? -wb.extents.x : wb.extents.x,
                    (i & 2) == 0 ? -wb.extents.y : wb.extents.y,
                    (i & 4) == 0 ? -wb.extents.z : wb.extents.z);
                Vector3 local = root.transform.InverseTransformPoint(corner);

                if (!has) { b = new Bounds(local, Vector3.zero); has = true; }
                else b.Encapsulate(local);
            }
        }

        center = b.center;
        top = b.max.y;
        return has;
    }

    static float SafeY(Vector3 v)
    {
        return Mathf.Approximately(v.y, 0f) ? 1f : v.y;
    }

    static Vector3 Div(Vector3 a, Vector3 b)
    {
        return new Vector3(
            Mathf.Approximately(b.x, 0f) ? a.x : a.x / b.x,
            Mathf.Approximately(b.y, 0f) ? a.y : a.y / b.y,
            Mathf.Approximately(b.z, 0f) ? a.z : a.z / b.z);
    }
}
