using TMPro;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Put this on the ROOT of an element prefab (the object with the collider).
// Quick tap on the element = nametag expands with info. Tap again = back to just the name.
// Holding (longer than tapMaxTime) is ignored, so your grab/hold logic is not affected.
public class ElementNametagTap : MonoBehaviour
{
    [Header("Optional: auto-found if left empty")]
    public TMP_Text label;
    [Tooltip("Fallback name if the label text is not an element name. The editor script fills this.")]
    public string elementName;

    [Header("Tap settings")]
    public float tapMaxTime = 0.25f;     // a press shorter than this counts as a tap
    public float tapMaxMovePixels = 25f; // finger can't move more than this

    [Header("Expanded look")]
    public Vector2 expandedSize = new Vector2(320, 150);
    public float expandedFontSize = 18f;

    bool expanded;
    bool pressing;
    float pressTime;
    Vector2 pressPos;

    string collapsedText;
    float collapsedFontSize;
    Vector2 collapsedLabelSize;
    Vector2 collapsedBgSize;
    TextWrappingModes collapsedWrap;

    RectTransform labelRect;
    RectTransform bgRect;

    void Awake()
    {
        if (label == null) label = GetComponentInChildren<TMP_Text>(true);
        if (label != null)
        {
            labelRect = label.rectTransform;
            bgRect = label.transform.parent as RectTransform; // the Image behind the text
        }
    }

    void Update()
    {
        if (label == null) return;

        Vector2 pos;
        if (PressedThisFrame(out pos))
        {
            pressing = true;
            pressTime = Time.time;
            pressPos = pos;
        }
        else if (pressing && ReleasedThisFrame(out pos))
        {
            pressing = false;
            bool quick = Time.time - pressTime <= tapMaxTime;
            bool still = Vector2.Distance(pos, pressPos) <= tapMaxMovePixels;
            if (quick && still && HitThisElement(pos)) Toggle();
        }
    }

    bool HitThisElement(Vector2 screenPos)
    {
        Camera cam = Camera.main;
        if (cam == null) return false;
        Ray ray = cam.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            return hit.transform == transform || hit.transform.IsChildOf(transform);
        return false;
    }

    void Toggle()
    {
        if (expanded) Collapse();
        else Expand();
    }

    void Expand()
    {
        // the label text is the default (collapsed) name, e.g. "Hydrogen"
        string current = label.text;
        if (!ElementData.TryGet(current, out ElementData.Info info) &&
            !ElementData.TryGet(elementName, out info))
            return; // not a single element (e.g. a compound), leave it alone

        collapsedText = current;
        collapsedFontSize = label.fontSize;
        collapsedWrap = label.textWrappingMode;
        collapsedLabelSize = labelRect.sizeDelta;
        if (bgRect != null) collapsedBgSize = bgRect.sizeDelta;

        label.text = info.number + "\n<b>" + info.symbol + " | " + info.name + "</b>\n" + info.description;
        label.textWrappingMode = TextWrappingModes.Normal;
        label.fontSize = expandedFontSize;
        labelRect.sizeDelta = expandedSize;
        if (bgRect != null) bgRect.sizeDelta = expandedSize;

        expanded = true;
    }

    void Collapse()
    {
        label.text = collapsedText;
        label.fontSize = collapsedFontSize;
        label.textWrappingMode = collapsedWrap;
        labelRect.sizeDelta = collapsedLabelSize;
        if (bgRect != null) bgRect.sizeDelta = collapsedBgSize;

        expanded = false;
    }

    // ---- input helpers (work with both Input System and old Input Manager) ----
    static bool PressedThisFrame(out Vector2 pos)
    {
#if ENABLE_INPUT_SYSTEM
        var t = Touchscreen.current;
        if (t != null && t.primaryTouch.press.wasPressedThisFrame)
        {
            pos = t.primaryTouch.position.ReadValue();
            return true;
        }
        var m = Mouse.current;
        if (m != null && m.leftButton.wasPressedThisFrame)
        {
            pos = m.position.ReadValue();
            return true;
        }
#else
        if (Input.GetMouseButtonDown(0))
        {
            pos = Input.mousePosition;
            return true;
        }
#endif
        pos = default;
        return false;
    }

    static bool ReleasedThisFrame(out Vector2 pos)
    {
#if ENABLE_INPUT_SYSTEM
        var t = Touchscreen.current;
        if (t != null && t.primaryTouch.press.wasReleasedThisFrame)
        {
            pos = t.primaryTouch.position.ReadValue();
            return true;
        }
        var m = Mouse.current;
        if (m != null && m.leftButton.wasReleasedThisFrame)
        {
            pos = m.position.ReadValue();
            return true;
        }
#else
        if (Input.GetMouseButtonUp(0))
        {
            pos = Input.mousePosition;
            return true;
        }
#endif
        pos = default;
        return false;
    }
}
