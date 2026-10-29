using UnityEngine;

// Fake breathing for characters that have no skeleton.
// Put this on the ROOT of a character prefab. It floats up and down a little and tilts forward/back a little.
// No Animator needed. Tweak the numbers in the Inspector while the game is playing (then copy them back after you stop).
public class IdleSway : MonoBehaviour
{
    [Header("Up and down")]
    public float floatHeight = 0.03f;   // how far it moves up/down
    public float floatSpeed = 1.5f;     // higher = faster

    [Header("Tilt forward and back")]
    public float tiltAngle = 1.5f;      // degrees
    public float tiltSpeed = 1.0f;

    private Vector3 startPos;
    private Quaternion startRot;
    private float randomOffset;

    void Start()
    {
        // remember where the prefab was placed, we only move AROUND that spot
        startPos = transform.localPosition;
        startRot = transform.localRotation;
        randomOffset = Random.value * 10f; // so two characters don't sway in perfect sync
    }

    void Update()
    {
        float t = Time.time + randomOffset;

        float up = Mathf.Sin(t * floatSpeed) * floatHeight;
        float tilt = Mathf.Sin(t * tiltSpeed) * tiltAngle;

        transform.localPosition = startPos + new Vector3(0f, up, 0f);

        // tilts around X. If your character leans left/right instead of forward/back, change Euler(tilt, 0, 0) to Euler(0, 0, tilt)
        transform.localRotation = startRot * Quaternion.Euler(tilt, 0f, 0f);
    }
}
