using UnityEngine;

public class ElementerFloat : MonoBehaviour
{
    public float floatHeight = 0.2f;
    public float floatSpeed = 1f;
    public float rotateSpeed = 30f;

    private Vector3 startPos;

    void Start()
    {
        float groundOffset = GetHeightOffset();
        startPos = transform.localPosition + Vector3.up * groundOffset;
    }

    float GetHeightOffset()
    {
        Renderer rend = GetComponentInChildren<Renderer>();
        if (rend == null) return 0f;

        // distance from pivot to the bottom of the mesh
        float bottomToPivot = transform.position.y - rend.bounds.min.y;
        return bottomToPivot;
    }

    void Update()
    {
        float offset = (Mathf.Sin(Time.time * floatSpeed) + 1f) / 2f * floatHeight;
        float newY = startPos.y + offset;
        transform.localPosition = new Vector3(startPos.x, newY, startPos.z);

        transform.Rotate(Vector3.right, rotateSpeed * Time.deltaTime);
    }
}