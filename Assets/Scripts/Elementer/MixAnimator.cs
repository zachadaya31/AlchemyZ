using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// "Pokemon evolution" style mix animation for AlchemiX.
/// 1) ORBIT  : every element swoops toward the LAST spawned element and circles it, spinning faster and faster.
/// 2) CHARGE : orbit tightens, everything glows white, grows a bit, screen starts to flash.
/// 3) PEAK   : screen is fully white -> the result is spawned (elements are destroyed by MixManager).
/// 4) REVEAL : white flash fades out while the result pops in with an overshoot.
/// </summary>
public class MixAnimator : MonoBehaviour
{
    [Header("Timing (seconds)")]
    public float orbitDuration = 1.8f;
    public float chargeDuration = 0.9f;
    public float revealDuration = 0.9f;

    [Header("Orbit")]
    [Tooltip("Orbit radius in meters around the last spawned element.")]
    public float orbitRadius = 0.35f;
    [Tooltip("Degrees per second at the start of the orbit.")]
    public float startSpinSpeed = 200f;
    [Tooltip("Degrees per second at the end of the orbit.")]
    public float maxSpinSpeed = 1500f;
    [Tooltip("Spreads orbiters at different heights so they don't overlap.")]
    public float heightSpread = 0.15f;

    [Header("Charge / Glow")]
    [Tooltip("How much bigger the elements get right before the flash (1 = no change).")]
    public float chargeScaleMultiplier = 1.3f;
    public float emissionIntensity = 3f;
    public float maxLightIntensity = 8f;
    public float lightRange = 3f;

    [Header("Optional")]
    [Tooltip("Particle burst spawned at the moment of the flash (optional).")]
    public GameObject burstPrefab;
    [Tooltip("Full-screen white CanvasGroup. Leave empty and one is created automatically.")]
    public CanvasGroup flashGroup;

    public bool IsPlaying { get; private set; }

    // everything we need to animate one element
    private class Orbiter
    {
        public Transform tr;
        public Vector3 baseScale;
        public float angle;      // degrees
        public float startRadius;
        public float startY;
        public float radiusMul;
        public float heightOffset;
        public List<MatInfo> mats = new List<MatInfo>();
    }

    private struct MatInfo
    {
        public Material mat;
        public string colorProp;
        public Color baseColor;
    }

    /// <param name="elements">Spawned elements in spawn order. The LAST one is the orbit center.</param>
    /// <param name="onPeak">Called when the screen is fully white. Should spawn the result and return it.</param>
    /// <param name="onDone">Called when the whole animation is finished.</param>
    public void Play(List<GameObject> elements, Func<GameObject> onPeak, Action onDone)
    {
        if (IsPlaying) return;
        StartCoroutine(PlayRoutine(elements, onPeak, onDone));
    }

    private IEnumerator PlayRoutine(List<GameObject> elements, Func<GameObject> onPeak, Action onDone)
    {
        IsPlaying = true;
        EnsureFlashOverlay();

        // ---------- setup ----------
        List<GameObject> valid = elements.FindAll(e => e != null);
        if (valid.Count == 0)
        {
            GameObject r = onPeak?.Invoke();
            IsPlaying = false;
            onDone?.Invoke();
            yield break;
        }

        GameObject centerObj = valid[valid.Count - 1];          // last spawned
        Transform center = centerObj.transform;
        Vector3 centerPos = center.position;
        Vector3 centerBaseScale = center.localScale;
        List<MatInfo> centerMats = CollectMaterials(centerObj);
        DisableFloat(centerObj);

        List<Orbiter> orbiters = new List<Orbiter>();
        int count = valid.Count - 1;
        for (int i = 0; i < count; i++)
        {
            GameObject go = valid[i];
            DisableFloat(go);

            Vector3 offset = go.transform.position - centerPos;
            Vector2 flat = new Vector2(offset.x, offset.z);

            Orbiter o = new Orbiter();
            o.tr = go.transform;
            o.baseScale = go.transform.localScale;
            o.startRadius = flat.magnitude;
            // if two elements were spawned on the same spot, spread them evenly
            o.angle = flat.sqrMagnitude > 0.0001f
                ? Mathf.Atan2(flat.y, flat.x) * Mathf.Rad2Deg
                : (360f / Mathf.Max(1, count)) * i;
            o.startY = go.transform.position.y;
            o.radiusMul = 1f + 0.12f * (i % 3);
            o.heightOffset = (i - (count - 1) * 0.5f) * heightSpread;
            o.mats = CollectMaterials(go);
            orbiters.Add(o);
        }

        Light glow = CreateGlowLight(centerPos);

        // ---------- 1) ORBIT ----------
        float t = 0f;
        while (t < orbitDuration)
        {
            float dt = Time.deltaTime;
            t += dt;
            float p = Mathf.Clamp01(t / orbitDuration);

            float speed = Mathf.Lerp(startSpinSpeed, maxSpinSpeed, p * p);
            float gather = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(p / 0.4f)); // swoop in during first 40%

            foreach (Orbiter o in orbiters)
            {
                if (o.tr == null) continue;
                float r = Mathf.Lerp(o.startRadius, orbitRadius * o.radiusMul, gather);
                float y = Mathf.Lerp(o.startY, centerPos.y + o.heightOffset, gather);
                MoveOrbiter(o, centerPos, speed, dt, r, y);
            }

            if (center != null) center.Rotate(0f, speed * dt, 0f, Space.World);

            glow.intensity = maxLightIntensity * 0.2f * p;
            yield return null;
        }

        // ---------- 2) CHARGE ----------
        t = 0f;
        while (t < chargeDuration)
        {
            float dt = Time.deltaTime;
            t += dt;
            float p = Mathf.Clamp01(t / chargeDuration);
            float eased = p * p * p; // ease in

            float speed = maxSpinSpeed * (1f + 0.3f * p);
            float scaleMul = Mathf.Lerp(1f, chargeScaleMultiplier, eased);

            foreach (Orbiter o in orbiters)
            {
                if (o.tr == null) continue;
                float r = Mathf.Lerp(orbitRadius * o.radiusMul, 0.02f, eased);
                float y = Mathf.Lerp(centerPos.y + o.heightOffset, centerPos.y, eased);
                MoveOrbiter(o, centerPos, speed, dt, r, y);
                o.tr.localScale = o.baseScale * scaleMul;
                ApplyWhite(o.mats, p);
            }

            if (center != null)
            {
                center.Rotate(0f, speed * dt, 0f, Space.World);
                center.localScale = centerBaseScale * scaleMul;
                ApplyWhite(centerMats, p);
            }

            glow.intensity = Mathf.Lerp(maxLightIntensity * 0.2f, maxLightIntensity, eased);

            // screen only starts flashing in the last 40% of the charge
            float flashP = Mathf.Clamp01((p - 0.6f) / 0.4f);
            flashGroup.alpha = flashP * flashP;

            yield return null;
        }

        // ---------- 3) PEAK ----------
        flashGroup.alpha = 1f;

        if (burstPrefab != null)
        {
            GameObject burst = Instantiate(burstPrefab, centerPos, Quaternion.identity);
            SceneManager.MoveGameObjectToScene(burst, gameObject.scene);
            Destroy(burst, 3f);
        }

        // MixManager spawns the result and destroys the old elements here
        GameObject result = onPeak?.Invoke();

        Vector3 resultTargetScale = Vector3.one;
        if (result != null)
        {
            resultTargetScale = result.transform.localScale;
            result.transform.localScale = Vector3.zero;
        }

        // ---------- 4) REVEAL ----------
        t = 0f;
        while (t < revealDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / revealDuration);

            flashGroup.alpha = (1f - p) * (1f - p);
            glow.intensity = maxLightIntensity * (1f - p);

            if (result != null)
                result.transform.localScale = resultTargetScale * EaseOutBack(p);

            yield return null;
        }

        if (result != null) result.transform.localScale = resultTargetScale;
        flashGroup.alpha = 0f;
        if (glow != null) Destroy(glow.gameObject);

        IsPlaying = false;
        onDone?.Invoke();
    }

    // ---------- helpers ----------

    private void MoveOrbiter(Orbiter o, Vector3 centerPos, float speed, float dt, float radius, float y)
    {
        o.angle += speed * dt;
        float rad = o.angle * Mathf.Deg2Rad;
        o.tr.position = new Vector3(
            centerPos.x + Mathf.Cos(rad) * radius,
            y,
            centerPos.z + Mathf.Sin(rad) * radius);
        o.tr.Rotate(0f, speed * dt, 0f, Space.World); // spin on its own axis too
    }

    // ElementFloat makes elements bob up and down; it would fight the animation
    private void DisableFloat(GameObject go)
    {
        ElementFloat f = go.GetComponent<ElementFloat>();
        if (f != null) f.enabled = false;
    }

    private List<MatInfo> CollectMaterials(GameObject go)
    {
        List<MatInfo> list = new List<MatInfo>();
        foreach (Renderer rend in go.GetComponentsInChildren<Renderer>())
        {
            foreach (Material m in rend.materials) // .materials = instanced copies, safe to edit
            {
                string prop = m.HasProperty("_BaseColor") ? "_BaseColor"
                            : m.HasProperty("_Color") ? "_Color" : null;

                MatInfo info = new MatInfo { mat = m, colorProp = prop };
                if (prop != null) info.baseColor = m.GetColor(prop);

                if (m.HasProperty("_EmissionColor")) m.EnableKeyword("_EMISSION");
                list.Add(info);
            }
        }
        return list;
    }

    private void ApplyWhite(List<MatInfo> mats, float amount)
    {
        foreach (MatInfo info in mats)
        {
            if (info.mat == null) continue;
            if (info.colorProp != null)
                info.mat.SetColor(info.colorProp, Color.Lerp(info.baseColor, Color.white, amount));
            if (info.mat.HasProperty("_EmissionColor"))
                info.mat.SetColor("_EmissionColor", Color.white * (amount * emissionIntensity));
        }
    }

    private Light CreateGlowLight(Vector3 pos)
    {
        GameObject go = new GameObject("MixGlowLight");
        go.transform.position = pos + Vector3.up * 0.2f;
        SceneManager.MoveGameObjectToScene(go, gameObject.scene);
        Light l = go.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = Color.white;
        l.range = lightRange;
        l.intensity = 0f;
        return l;
    }

    // builds a full-screen white overlay if you didn't assign one
    private void EnsureFlashOverlay()
    {
        if (flashGroup != null) return;

        GameObject canvasGO = new GameObject("MixFlashCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        flashGroup = canvasGO.AddComponent<CanvasGroup>();
        flashGroup.alpha = 0f;
        flashGroup.blocksRaycasts = false;
        flashGroup.interactable = false;

        GameObject imgGO = new GameObject("Flash");
        imgGO.transform.SetParent(canvasGO.transform, false);
        Image img = imgGO.AddComponent<Image>();
        img.color = Color.white;
        img.raycastTarget = false;
        RectTransform rt = img.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        SceneManager.MoveGameObjectToScene(canvasGO, gameObject.scene);
    }

    private static float EaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
    }
}
