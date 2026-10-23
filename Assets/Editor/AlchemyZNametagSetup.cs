using TMPro;
using UnityEditor;
using UnityEngine;
public static class AlchemyZNametagSetup
{
    const string ElementsFolder = "Assets/Resources/Elements";

    // Bigger number = bigger nametag
    const float SizeMultiplier = 1f;

    // Gap between the top of the model and the nametag, in meters (smaller = closer)
    const float Gap = 0.01f;

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

        // Base size comes from the template. If the template already has NametagFixedSize,
        // use its base size so re-running never makes the tags bigger and bigger.
        NametagFixedSize templateFixed = template.GetComponent<NametagFixedSize>();
        Vector3 baseScale = templateFixed != null ? templateFixed.baseScale : template.lossyScale;

        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { ElementsFolder });
        int tagsAdded = 0, tapsAdded = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                Transform old = root.transform.Find("Nametag");
                if (old != null) Object.DestroyImmediate(old.gameObject);

                GameObject tag = Object.Instantiate(template.gameObject, root.transform);
                tag.name = "Nametag";
                tag.transform.localPosition = template.localPosition;
                tag.transform.localRotation = Quaternion.identity;

                NametagFixedSize fixedSize = tag.GetComponent<NametagFixedSize>();
                if (fixedSize == null) fixedSize = tag.AddComponent<NametagFixedSize>();
                fixedSize.baseScale = baseScale;
                fixedSize.sizeMultiplier = SizeMultiplier;
                fixedSize.gap = Gap;

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
}
