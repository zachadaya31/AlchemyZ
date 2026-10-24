using System.Collections;
using UnityEngine;
using TMPro;

public class FlashingText : MonoBehaviour
{
    [SerializeField] private float interval = 0.1f;
    [SerializeField] private int flashCount = 5; // Number of times to flash

    private Coroutine flashCoroutine;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        // Ensure we have a CanvasGroup to control visibility smoothly without killing the GameObject
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    private void OnEnable()
    {
        TriggerFlash();
    }

    private void OnDisable()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }
        if (canvasGroup != null) canvasGroup.alpha = 1f;
    }

    // Call this to restart the flashing sequence anytime
    public void TriggerFlash()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }
        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        var wait = new WaitForSeconds(interval);

        // Ensure it's fully visible at the start
        canvasGroup.alpha = 1f;

        // Perform the flashing cycle for the specified count
        for (int i = 0; i < flashCount; i++)
        {
            // Turn off visibility
            canvasGroup.alpha = 0f;
            yield return wait;

            // Turn on visibility
            canvasGroup.alpha = 1f;
            yield return wait;
        }

        // After 5 flashes are done, completely deactivate the warning GameObject
        flashCoroutine = null;
        gameObject.SetActive(false);
    }
}