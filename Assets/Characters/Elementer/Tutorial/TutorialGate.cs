using UnityEngine;

public class TutorialGate : MonoBehaviour
{
    public GameObject tutorialManager;
    public GameObject tutorialInstructions; // off din toh para mag on lang pag may tutorial

    async void Start()
    {
        bool finished = false;

        if (SessionManager.Instance != null && SupabaseManager.Instance != null)
        {
            try
            {
                string id = SessionManager.Instance.studentID.ToString();

                var res = await SupabaseManager.Instance.supabase
                    .From<Student_Tutorial_Elementer>()
                    .Where(r => r.studentID == id)
                    .Get();

                Debug.Log("TutorialGate: found " + res.Models.Count + " record(s) for student " + id);
                finished = res.Models.Exists(m => m.hasFinishedTutorial);
            }
            catch (System.Exception e)
            {
                Debug.LogError("TutorialGate: could not check record: " + e.Message);
            }
        }
        else
        {
            Debug.Log("TutorialGate: no login found, showing the tutorial.");
            tutorialInstructions.SetActive(true);
        }

        if (this == null) return;
        tutorialManager.SetActive(!finished);
        tutorialInstructions.SetActive(!finished); 
        
    }
}