using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Full screen loading overlay with the same look as the login one (dark layer, gold spinner, "Loading..." text).
// Use it from anywhere:   LoadingScreen.Load("MainMenu");
// (SceneLoader.loadScene() already uses it, so you usually don't call this directly.)
// It builds itself in code and survives scene changes, so there is nothing to set up in Unity.
public class LoadingScreen : MonoBehaviour
{
    private static LoadingScreen instance;

    private GameObject overlay;
    private Image spinner;
    private TextMeshProUGUI text;
    private string message = "Loading";
    private int lastDots = -1;
    private bool loading;

    // how long the screen stays up at least, so it doesn't just flash on fast loads
    private const float minimumSeconds = 0.8f;

    public static void Load(string sceneName, string message = "Loading")
    {
        if (instance == null)
        {
            GameObject go = new GameObject("LoadingScreen");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<LoadingScreen>();
        }
        instance.StartLoad(sceneName, message);
    }

    private void StartLoad(string sceneName, string msg)
    {
        if (loading) return; // ignore double taps

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError("Scene '" + sceneName + "' is not in Build Settings.");
            return;
        }

        loading = true;
        message = msg;
        lastDots = -1;
        BuildOverlay();
        overlay.SetActive(true);
        StartCoroutine(LoadRoutine(sceneName));
    }

    private IEnumerator LoadRoutine(string sceneName)
    {
        float startTime = Time.unscaledTime;

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        while (op != null && !op.isDone)
        {
            yield return null;
        }

        // new scene is loaded now, keep the overlay a little longer so its Start() stuff can finish behind it
        while (Time.unscaledTime - startTime < minimumSeconds)
        {
            yield return null;
        }

        overlay.SetActive(false);
        loading = false;
    }

    void Update()
    {
        if (overlay == null || !overlay.activeSelf) return;

        spinner.rectTransform.Rotate(0f, 0f, 360f * Time.unscaledDeltaTime);

        // "Loading" -> "Loading." -> ".." -> "..."
        int dots = (int)(Time.unscaledTime * 2.5f) % 4;
        if (dots != lastDots)
        {
            lastDots = dots;
            // invisible dots keep the text from jittering sideways
            text.SetText(message + new string('.', dots) + "<alpha=#00>" + new string('.', 3 - dots));
        }
    }

    // ---------- build the overlay (same as the login one) ----------

    private void BuildOverlay()
    {
        if (overlay != null) return;

        GameObject canvasGO = new GameObject("LoadingOverlay");
        canvasGO.transform.SetParent(transform, false);

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 2000; // above everything, even the login overlay

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>(); // lets the dark layer block taps underneath

        // dark full-screen layer
        GameObject dimGO = new GameObject("Dim");
        dimGO.transform.SetParent(canvasGO.transform, false);
        Image dim = dimGO.AddComponent<Image>();
        dim.color = new Color(0f, 0f, 0f, 0.9f);
        RectTransform dimRT = dim.rectTransform;
        dimRT.anchorMin = Vector2.zero;
        dimRT.anchorMax = Vector2.one;
        dimRT.offsetMin = Vector2.zero;
        dimRT.offsetMax = Vector2.zero;

        // spinner
        GameObject spinGO = new GameObject("Spinner");
        spinGO.transform.SetParent(canvasGO.transform, false);
        spinner = spinGO.AddComponent<Image>();
        spinner.sprite = MakeRingSprite();
        spinner.color = new Color(1f, 0.85f, 0.45f, 1f); // gold, matches the UI
        spinner.raycastTarget = false;
        spinner.rectTransform.sizeDelta = new Vector2(170f, 170f);
        spinner.rectTransform.anchoredPosition = new Vector2(0f, 60f);

        // text
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(canvasGO.transform, false);
        text = textGO.AddComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 52f;
        text.color = Color.white;
        text.raycastTarget = false;
        // reuse the game's font if we can find a text in the current scene
        TextMeshProUGUI anyText = FindAnyObjectByType<TextMeshProUGUI>();
        if (anyText != null) text.font = anyText.font;
        text.rectTransform.sizeDelta = new Vector2(900f, 110f);
        text.rectTransform.anchoredPosition = new Vector2(0f, -110f);

        overlay = canvasGO;
        overlay.SetActive(false);
    }

    // draws a ring with a fading tail so it looks like a spinner, no image asset needed
    private static Sprite MakeRingSprite()
    {
        const int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;

        Vector2 c = new Vector2(size / 2f, size / 2f);
        float outer = size / 2f - 1f;
        float inner = outer - size * 0.12f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 p = new Vector2(x, y) - c;
                float d = p.magnitude;
                float a = 0f;

                if (d <= outer + 0.5f && d >= inner - 0.5f)
                {
                    float angle = Mathf.Atan2(p.y, p.x) * Mathf.Rad2Deg; // -180..180
                    a = (angle + 180f) / 360f;                           // fading tail
                    a *= Mathf.Clamp01(outer - d + 0.5f) * Mathf.Clamp01(d - inner + 0.5f);
                }
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
}
