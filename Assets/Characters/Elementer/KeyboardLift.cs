using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Lifts UI panels so the focused TMP_InputField is never hidden behind the mobile keyboard.
/// The focused field MUST be inside one of the lifted panels (target / extraTargets),
/// otherwise lifting can't bring it into view and the script ignores it.
/// </summary>
public class KeyboardLift : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Main panel to move up (Frame).")]
    public RectTransform target;
    [Tooltip("Other panels that must move together with it (e.g. Element Library). Anything holding an input field goes here.")]
    public RectTransform[] extraTargets;
    public Canvas canvas;

    [Header("Settings")]
    [Tooltip("Gap in screen pixels between the input field and the top of the keyboard.")]
    public float padding = 40f;
    public float smoothSpeed = 12f;
    [Tooltip("Never lift more than this fraction of the screen height.")]
    [Range(0.1f, 1f)] public float maxLiftPercent = 0.5f;

    [Header("Editor testing")]
    [Tooltip("Fake keyboard height as a fraction of the Game view height. A real phone keyboard is about 0.35 - 0.45. 0 = off.")]
    [Range(0f, 0.6f)] public float editorFakeKeyboardPercent = 0f;

    private RectTransform[] all;
    private Vector2[] startPositions;
    private float currentLiftPx; // SCREEN pixels lifted right now
    private bool warnedOutside;
    private readonly Vector3[] corners = new Vector3[4];

#if UNITY_ANDROID && !UNITY_EDITOR
    private int baselineInset;
#endif

    void Start()
    {
        if (canvas == null && target != null) canvas = target.GetComponentInParent<Canvas>();

        var list = new System.Collections.Generic.List<RectTransform>();
        if (target != null) list.Add(target);
        if (extraTargets != null)
            foreach (var t in extraTargets)
                if (t != null && !list.Contains(t)) list.Add(t);

        var filtered = new System.Collections.Generic.List<RectTransform>();
        foreach (var a in list)
        {
            bool nested = false;
            foreach (var b in list)
                if (a != b && a.IsChildOf(b)) { nested = true; break; }
            if (!nested) filtered.Add(a);
        }
        all = filtered.ToArray();
        
        startPositions = new Vector2[all.Length];
        for (int i = 0; i < all.Length; i++) startPositions[i] = all[i].anchoredPosition;

#if UNITY_ANDROID && !UNITY_EDITOR
        baselineInset = GetAndroidBottomInset();
#endif
    }

    void LateUpdate()
    {
        if (all == null || all.Length == 0) return;

        float desired = 0f;

        TMP_InputField focused = GetFocusedInput();
        if (focused != null)
        {
            if (!IsInsideLiftedPanel(focused.transform))
            {
                if (!warnedOutside)
                {
                    warnedOutside = true;
                    Debug.LogWarning($"KeyboardLift: '{focused.name}' is not inside any lifted panel. Add its parent panel to Extra Targets.", focused);
                }
            }
            else
            {
                float kb = GetKeyboardHeightPx();
                if (kb > 0f)
                {
                    // where the field's bottom edge would be if nothing were lifted
                    float fieldBottomUnlifted = GetScreenBottom((RectTransform)focused.transform) - currentLiftPx;
                    desired = Mathf.Max(0f, kb + padding - fieldBottomUnlifted);
                    desired = Mathf.Min(desired, Screen.height * maxLiftPercent);
                }
            }
        }

        if (smoothSpeed <= 0f)
            currentLiftPx = desired;
        else
        {
            currentLiftPx = Mathf.Lerp(currentLiftPx, desired, Time.unscaledDeltaTime * smoothSpeed);
            if (Mathf.Abs(currentLiftPx - desired) < 0.5f) currentLiftPx = desired;
        }

        float scale = canvas != null ? canvas.rootCanvas.scaleFactor : 1f;
        for (int i = 0; i < all.Length; i++)
            all[i].anchoredPosition = startPositions[i] + new Vector2(0f, currentLiftPx / scale);
    }

    private bool IsInsideLiftedPanel(Transform t)
    {
        foreach (var panel in all)
            if (t == panel || t.IsChildOf(panel)) return true;
        return false;
    }

    private TMP_InputField GetFocusedInput()
    {
        if (EventSystem.current == null) return null;
        GameObject sel = EventSystem.current.currentSelectedGameObject;
        if (sel == null) return null;
        TMP_InputField input = sel.GetComponent<TMP_InputField>();
        return (input != null && input.isFocused) ? input : null;
    }

    private float GetKeyboardHeightPx()
    {
#if UNITY_EDITOR
        return Screen.height * editorFakeKeyboardPercent;
#else
        float h = 0f;
        if (TouchScreenKeyboard.visible) h = TouchScreenKeyboard.area.height;
#if UNITY_ANDROID
        // Unity often reports 0 on Android, so ask the OS directly
        if (h <= 0f) h = Mathf.Max(0, GetAndroidBottomInset() - baselineInset);
#endif
        return h;
#endif
    }

    // lowest point of a RectTransform in screen pixels
    private float GetScreenBottom(RectTransform rt)
    {
        rt.GetWorldCorners(corners);
        Canvas root = canvas.rootCanvas;
        Camera cam = root.renderMode == RenderMode.ScreenSpaceOverlay ? null : root.worldCamera;

        float lowest = float.MaxValue;
        for (int i = 0; i < 4; i++)
        {
            float y = RectTransformUtility.WorldToScreenPoint(cam, corners[i]).y;
            if (y < lowest) lowest = y;
        }
        return lowest;
    }



#if UNITY_ANDROID && !UNITY_EDITOR
    // pixels at the bottom of the window that are NOT visible (keyboard + nav bar)
    private static int GetAndroidBottomInset()
    {
        try
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var window = activity.Call<AndroidJavaObject>("getWindow"))
            using (var decor = window.Call<AndroidJavaObject>("getDecorView"))
            using (var rect = new AndroidJavaObject("android.graphics.Rect"))
            {
                decor.Call("getWindowVisibleDisplayFrame", rect);
                return decor.Call<int>("getHeight") - rect.Get<int>("bottom");
            }
        }
        catch
        {
            return 0;
        }
    }

    
#endif
}