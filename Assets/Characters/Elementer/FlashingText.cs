using System.Collections;
using UnityEngine;
using TMPro;

public class FlashingText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private float interval = 0.5f;

    private void Awake()
    {
        if (text == null) text = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        StartCoroutine(Flash());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        text.enabled = true; // reset so it isn't left hidden
    }

    private IEnumerator Flash()
    {
        var wait = new WaitForSeconds(interval);
        while (true)
        {
            text.enabled = !text.enabled;
            yield return wait;
        }
    }
}