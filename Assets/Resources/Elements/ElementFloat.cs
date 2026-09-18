using UnityEngine;

public class ElementFloat : MonoBehaviour
{
    public float bobHeight = 0.15f;
    public float bobSpeed = 1.5f;
    public float groundOffset = 0.15f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position + new Vector3(0, groundOffset, 0);
    }

    void Update()
    {
        float newY = startPos.y + (Mathf.Sin(Time.time * bobSpeed) + 1f) * 0.5f * bobHeight;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }
}