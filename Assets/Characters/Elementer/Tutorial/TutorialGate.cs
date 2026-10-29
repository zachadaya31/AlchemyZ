using System.Threading.Tasks;
using UnityEngine;

// Shows the tutorial only if the student has no "finished" record in the new database.
public class TutorialGate : MonoBehaviour
{
    public GameObject tutorialManager;
    public GameObject tutorialInstructions;

    async void Start()
    {
        // hide both until we know the answer (so the tutorial doesn't flash for returning students)
        tutorialManager.SetActive(false);
        tutorialInstructions.SetActive(false);

        bool finished = false;

        if (SupabaseManager.Instance != null)
        {
            // SupabaseManager connects in its own Awake, wait (max ~5s) until it is ready
            float t = 0f;
            while (SupabaseManager.Instance.supabase == null && t < 5f)
            {
                await Task.Yield();
                t += Time.unscaledDeltaTime;
            }

            if (SupabaseManager.Instance.supabase != null)
            {
                finished = await SupabaseManager.Instance.HasFinishedTutorial();
            }
        }

        Debug.Log("TutorialGate: finished = " + finished);

        if (this == null) return;
        tutorialManager.SetActive(!finished);
        tutorialInstructions.SetActive(!finished);
    }

    // CALL THIS when the tutorial's last step is done, ex: FindObjectOfType<TutorialGate>().CompleteTutorial();
    // (or call SupabaseManager.Instance.MarkTutorialFinished() directly)
    public async void CompleteTutorial()
    {
        await SupabaseManager.Instance.MarkTutorialFinished();
        Debug.Log("TutorialGate: tutorial saved as finished.");
    }
}