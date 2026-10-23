using UnityEngine;
using UnityEngine.UI;

// Goes on the Nametag object (the Editor setup script adds it for you).
// Every frame it:
//   1. keeps the nametag at the SAME world size, no matter how the parent element is scaled
//   2. places it just above the top of the element's model
public class NametagFixedSize : MonoBehaviour
{
    [Header("Size (world scale = baseScale * sizeMultiplier)")]
    public Vector3 baseScale = Vector3.one;
    public float sizeMultiplier = 2.5f;

    [Header("Gap between the model's top and the nametag, in meters")]
    public float gap = 0.01f;

    Transform owner;
    Renderer[] renderers;
    RectTransform bg;

    void Start()
    {
        owner = transform.parent;
        Image img = GetComponentInChildren<Image>(true);
        if (img != null) bg = img.rectTransform;
    }

    void LateUpdate()
    {
        if (owner == null) return;
        if (renderers == null) renderers = owner.GetComponentsInChildren<Renderer>();

        // 1. fixed world size
        Vector3 ps = owner.lossyScale;
        Vector3 target = baseScale * sizeMultiplier;
        transform.localScale = new Vector3(target.x / NotZero(ps.x), target.y / NotZero(ps.y), target.z / NotZero(ps.z));

        // 2. sit on top of the model
        bool has = false;
        Bounds b = new Bounds();
        foreach (Renderer r in renderers)
        {
            if (r == null || !r.enabled || r.transform.IsChildOf(transform)) continue;
            if (!has) { b = r.bounds; has = true; }
            else b.Encapsulate(r.bounds);
        }
        if (!has) return;

        float halfHeight = 0f;
        float centerOffset = 0f;
        if (bg != null)
        {
            halfHeight = bg.rect.height * bg.lossyScale.y * 0.5f;
            centerOffset = bg.TransformPoint(bg.rect.center).y - transform.position.y;
        }

        float centerY = b.max.y + gap + halfHeight;
        transform.position = new Vector3(b.center.x, centerY - centerOffset, b.center.z);
    }

    static float NotZero(float v)
    {
        return Mathf.Approximately(v, 0f) ? 1f : v;
    }
}
