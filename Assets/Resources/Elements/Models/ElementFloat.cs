using UnityEngine;

public class ElementFloat : MonoBehaviour
{
    public float bobHeight = 0.15f;
    public float bobSpeed = 1.5f;
    public float groundOffset = 0.15f;

    private Vector3 startPos;
    private bool started = false;

    // The spot on the ground (plane) where this object was placed, ignoring the bobbing.
    // MixManager spawns the mixture here so it floats at the same height as the elements.
    public Vector3 GroundPosition
    {
        get
        {
            if (!started) return transform.position;
            return new Vector3(startPos.x, startPos.y - groundOffset, startPos.z);
        }
    }

    void Start()
    {
        startPos = transform.position + new Vector3(0, groundOffset, 0);
        started = true;
    }

    void Update()
    {
        float newY = startPos.y + (Mathf.Sin(Time.time * bobSpeed) + 1f) * 0.5f * bobHeight;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }
}