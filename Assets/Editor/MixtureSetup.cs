using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

// Put this file inside a folder called "Editor" (e.g. Assets/Editor/MixtureSetup.cs).
// Put MixtureNametagTap.cs in your normal Scripts folder (NOT in Editor).
//
// Menu: AlchemyZ > Setup Mixture Prefabs
// Before running: select a prefab (or Nametag) that already has a Nametag, like Hydrogen.
// It does these things to every prefab in Assets/Resources/Mixtures:
//   1. removes Lights (only if the prefab has them)
//   2. adds a BoxCollider (only if the root has none)
//   3. adds ElementScale so you can fix sizes with the Size Multiplier
//   4. adds the Nametag + tap info (info text comes from the table below)
//   5. prints each item's size in meters in the Console so you can compare them
// Safe to run again. It will not stack up duplicates.
public static class MixtureSetup
{
    const string MixturesFolder = "Assets/Resources/Mixtures";

    // Bigger number = bigger nametag (same as the element setup)
    const float SizeMultiplier = 1f;

    // Gap between the top of the model and the nametag, in meters
    const float Gap = 0.01f;

    // Format: Name;Other name|info text
    // The name must match the prefab file name (spaces, dashes, underscores and capitals are ignored).
    static readonly string[] infoRows =
    {
        "Water|Made of 2 hydrogen atoms and 1 oxygen atom. It covers about 71% of Earth's surface and all known life needs it.",
        "Hydrogen Peroxide|Has one more oxygen atom than water. Diluted, it is used to disinfect cuts and bleach hair.",
        "Carbon Dioxide|A gas made of 1 carbon and 2 oxygen atoms. Plants absorb it for photosynthesis and we breathe it out.",
        "Carbon Monoxide|A colorless, odorless, poisonous gas made when fuel burns without enough oxygen.",
        "Methane|1 carbon bonded to 4 hydrogen atoms. It is the main part of natural gas used for cooking and heating.",
        "Ethane|A colorless gas with 2 carbon and 6 hydrogen atoms. It is found in natural gas and used to make plastics.",
        "Propane|A fuel gas with 3 carbon and 8 hydrogen atoms. It is stored as a liquid in tanks for stoves and heaters.",
        "Butane|A fuel gas with 4 carbon and 10 hydrogen atoms. It is used in lighters and portable camping stoves.",
        "Ammonia|1 nitrogen bonded to 3 hydrogen atoms. This sharp-smelling gas is used to make fertilizers and cleaners.",
        "Nitric Oxide|A gas made in car engines and lightning. Our body also uses it to relax blood vessels.",
        "Nitrogen Dioxide|A reddish-brown toxic gas from vehicle exhaust. It adds to smog and acid rain.",
        "Nitrous Oxide|Known as laughing gas. Dentists use it as a mild anesthetic.",
        "Ozone|3 oxygen atoms bonded together. The ozone layer high in the sky blocks harmful UV rays from the Sun.",
        "Hydrogen Sulfide|A toxic gas that smells like rotten eggs.",
        "Sulfur Dioxide;Sulfur Oxide|A sharp-smelling gas released by volcanoes and burning coal. It is a cause of acid rain.",
        "Sulfur Trioxide|Reacts with water to make sulfuric acid, so it is very important in industry.",
        "Silicon Dioxide|The main part of sand and quartz, and the main material used to make glass.",
        "Glucose|A simple sugar that plants make by photosynthesis. Our cells use it as a main source of energy.",
        "Ethanol|The alcohol found in drinks. It is also used as a fuel and in hand sanitizer.",
        "Acetic Acid|The acid that gives vinegar its sour taste and sharp smell.",
        "Hydrochloric Acid|A strong acid. Your stomach makes a diluted form of it to help digest food.",
        "Hydrofluoric Acid|A weak acid but a very dangerous one. It can even etch glass.",
        "Sulfuric Acid|A strong acid used in car batteries and fertilizer production. It is one of the most widely made chemicals in the world.",
        "Nitric Acid|A strong acid used to make fertilizers and explosives.",
        "Carbonic Acid|Forms when carbon dioxide dissolves in water. It gives soda its fizz.",
        "Phosphoric Acid|Used to make fertilizers. It also gives cola its tangy taste.",
        "Sodium Hydroxide|Also called lye or caustic soda. This strong base is used to make soap and unclog drains.",
        "Potassium Hydroxide|A strong base known as caustic potash. It is used to make soft soap and alkaline batteries.",
        "Calcium Hydroxide|Also called slaked lime. It is used in cement and plaster and to treat acidic soil.",
        "Magnesium Hydroxide|The main ingredient in milk of magnesia, which is used to relieve heartburn.",
        "Table Salt;Sodium Chloride;Salt|1 sodium atom and 1 chlorine atom. Sodium is a reactive metal and chlorine is a poisonous gas, but together they are safe to eat.",
        "Potassium Chloride|Used in fertilizers and as a salt substitute. Our nerves and muscles need potassium.",
        "Magnesium Chloride|Found in sea water. It is used to firm up tofu and to melt ice on roads.",
        "Calcium Chloride|Absorbs moisture from the air, so it is used as a drying agent and to melt ice on roads.",
        "Silver Chloride|A white solid that turns dark in light. It was used in old photographic film.",
        "Ammonium Chloride|Used in dry-cell batteries and as a fertilizer.",
        "Baking Soda;Sodium Bicarbonate|Gives off carbon dioxide gas when heated or mixed with an acid. That is what makes cakes rise.",
        "Washing Soda;Sodium Carbonate|Used in laundry to soften hard water and remove stains.",
        "Limestone;Calcium Carbonate|Found in chalk, marble, seashells and eggshells. Heating it makes quicklime.",
        "Copper Sulfate;Copper(II) Sulfate|Forms bright blue crystals when hydrated. It is used to kill algae and in school experiments.",
        "Sodium Nitrate|Used in fertilizers and as a preservative in cured meats.",
        "Potassium Nitrate;Saltpeter|Also called saltpeter. It is used in fertilizers, fireworks and gunpowder.",
        "Magnesium Oxide|A white powder made when magnesium burns. It is used to line furnaces because it handles very high heat.",
        "Calcium Oxide;Quicklime|Also called quicklime, made by heating limestone. It reacts strongly with water and is used in cement.",
        "Zinc Oxide|A white powder used in sunscreen and diaper rash creams.",
        "Copper(II) Oxide;Copper Oxide|A black solid made when copper is heated in air. It is used to color glass and ceramics.",
        "Iron(II) Oxide;Iron Oxide|A black solid and one of the iron oxides. It is used as a pigment.",
        "Iron(III) Oxide;Rust|Better known as rust. It is the reddish-brown coating that forms when iron reacts with oxygen and water.",
        "Aluminum Oxide;Aluminium Oxide|A very hard compound used in sandpaper. Rubies and sapphires are made of it too.",
        "Sodium Oxide|A white solid that reacts strongly with water to form sodium hydroxide."
    };

    static string Normalize(string s)
    {
        return s.Trim().ToLowerInvariant()
            .Replace(" ", "").Replace("-", "").Replace("_", "")
            .Replace("(", "").Replace(")", "");
    }

    static Dictionary<string, string> BuildInfoLookup()
    {
        var lookup = new Dictionary<string, string>();
        foreach (string row in infoRows)
        {
            string[] p = row.Split('|');
            foreach (string name in p[0].Split(';'))
                lookup[Normalize(name)] = p[1];
        }
        return lookup;
    }

    [MenuItem("AlchemyZ/Setup Mixture Prefabs")]
    static void Run()
    {
        // ---- find the Nametag template (same idea as the element setup) ----
        GameObject sel = Selection.activeGameObject;
        if (sel == null)
        {
            Debug.LogError("[AlchemyZ] Select a prefab or object that has a Nametag first (like Hydrogen), then run this again.");
            return;
        }

        Transform template = sel.name == "Nametag" ? sel.transform : sel.transform.Find("Nametag");
        if (template == null)
        {
            Debug.LogError("[AlchemyZ] The selected object has no child called 'Nametag'.");
            return;
        }

        NametagFixedSize templateFixed = template.GetComponent<NametagFixedSize>();
        Vector3 baseScale = templateFixed != null ? templateFixed.baseScale : template.lossyScale;

        var infoLookup = BuildInfoLookup();
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { MixturesFolder });

        int lightsRemoved = 0, lightsDisabled = 0, collidersAdded = 0, scalesAdded = 0, tagsAdded = 0, infoMissing = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string itemName = Path.GetFileNameWithoutExtension(path);
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                // clear any old nametag first so it never counts as part of the model
                Transform old = root.transform.Find("Nametag");
                if (old != null) Object.DestroyImmediate(old.gameObject);

                // 1. lights
                foreach (Light l in root.GetComponentsInChildren<Light>(true))
                {
                    GameObject go = l.gameObject;
                    // URP adds a helper component that blocks removing the Light, so remove that first
                    foreach (Component c in go.GetComponents<Component>())
                    {
                        if (c != null && c.GetType().Name == "UniversalAdditionalLightData")
                            Object.DestroyImmediate(c);
                    }
                    try { Object.DestroyImmediate(l); } catch (Exception) { }

                    if (l != null)
                    {
                        // could not be removed (part of a nested prefab), so just turn it off
                        l.enabled = false;
                        lightsDisabled++;
                    }
                    else
                    {
                        lightsRemoved++;
                        // delete the empty object Blender leaves behind
                        if (go != root && go.transform.childCount == 0 && go.GetComponents<Component>().Length == 1)
                            Object.DestroyImmediate(go);
                    }
                }

                // 2. box collider
                Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
                Bounds b = new Bounds(root.transform.position, Vector3.zero);
                if (renderers.Length > 0)
                {
                    b = renderers[0].bounds;
                    foreach (Renderer r in renderers) b.Encapsulate(r.bounds);

                    if (root.GetComponent<Collider>() == null)
                    {
                        BoxCollider col = root.AddComponent<BoxCollider>();
                        col.center = root.transform.InverseTransformPoint(b.center);
                        Vector3 s = root.transform.lossyScale;
                        col.size = new Vector3(b.size.x / s.x, b.size.y / s.y, b.size.z / s.z);
                        collidersAdded++;
                    }
                }
                else
                {
                    Debug.LogWarning("[AlchemyZ] No renderers (no mesh) in: " + path);
                }

                // 3. scale multiplier
                if (root.GetComponent<ElementScale>() == null)
                {
                    root.AddComponent<ElementScale>();
                    scalesAdded++;
                }

                // 4. nametag + tap info
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
                // "HydrogenPeroxide" shows as "Hydrogen Peroxide", "Iron(III)Oxide" as "Iron(III) Oxide"
                if (text != null) text.text = Regex.Replace(itemName, @"(?<=[a-z\)])(?=[A-Z])", " ");
                tagsAdded++;

                MixtureNametagTap tap = root.GetComponent<MixtureNametagTap>();
                if (tap == null) tap = root.AddComponent<MixtureNametagTap>();
                tap.label = null; // auto-found at runtime

                if (infoLookup.TryGetValue(Normalize(itemName), out string info))
                {
                    tap.info = info;
                }
                else if (string.IsNullOrEmpty(tap.info))
                {
                    infoMissing++;
                    Debug.LogWarning("[AlchemyZ] No info text for '" + itemName + "'. Fill it in on the prefab, or rename the prefab to match the table.");
                }

                // 5. size report (in meters, before the ElementScale multiplier)
                Debug.Log("[AlchemyZ] " + itemName + " size: "
                    + b.size.x.ToString("F3") + " x " + b.size.y.ToString("F3") + " x " + b.size.z.ToString("F3") + " m");

                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[AlchemyZ] Done. Prefabs: " + guids.Length
            + " | Lights removed: " + lightsRemoved + " (disabled instead: " + lightsDisabled + ")"
            + " | Colliders added: " + collidersAdded
            + " | ElementScale added: " + scalesAdded
            + " | Nametags: " + tagsAdded
            + " | Missing info: " + infoMissing);
    }
}
