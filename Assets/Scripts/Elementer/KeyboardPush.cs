using UnityEngine;

public class KeyboardPush : MonoBehaviour
{
    public Canvas canvas;          // drag your Canvas here
    private RectTransform rect;
    private float startY;

    void Start()
    {
        rect = GetComponent<RectTransform>();
        startY = rect.anchoredPosition.y;
    }

    void Update()
    {
        float push = 0f;

        if (TouchScreenKeyboard.visible)
        {
            push = TouchScreenKeyboard.area.height / canvas.scaleFactor;
        }

        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, startY + push);
    }
}