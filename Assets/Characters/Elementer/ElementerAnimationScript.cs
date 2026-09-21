using UnityEngine;

public class ElementerFloat : MonoBehaviour
{
    public float floatHeight = 0.2f;
    public float floatSpeed = 1f;
    public float rotateSpeed = 30f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.localPosition = new Vector3(startPos.x, newY, startPos.z);

        transform.Rotate(Vector3.right, rotateSpeed * Time.deltaTime);
    }
}