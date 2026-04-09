//using Unity.Android.Gradle;
using UnityEngine;

public class Mission1 : MonoBehaviour
{
    private int currentStep;

    public Dialogue dialogueScript;
    public GameObject teacherPrefab;
    public GameObject scientistPrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        nextStep();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void nextStep() {
        currentStep++;
        switch (currentStep) {
            // ----------------------- SCENE 1
            case 1: 
                string[] lines = {
                    "Okay class, for the next question...",
                    "2nd Line",
                    "3rd Line"
                };
                dialogueScript.loadConversation("Teacher Mikko", teacherPrefab, lines);
                break;

            // ----------------------- SCENE 2

            default:
                Debug.Log("Default");
                break;
        }
    }
}
