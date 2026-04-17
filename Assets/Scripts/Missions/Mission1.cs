//using Unity.Android.Gradle;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Mission1 : MonoBehaviour
{
    private int currentScene = 0;
    public static Mission1 Instance;

    public Dialogue dialogueLoader;
    public Animator fadeAnimator;
    public GameObject teacherPrefab;
    public Animator teacherAnimations;
    public GameObject scientistPrefab;

    [Header("Buttons")]
    public GameObject buttonPrefab;
    public Transform buttonsChoicesContainer;
    public TextMeshProUGUI questionPrefab;
    public Button backButton;
    public Button nextButton;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        nextScene();
    }

    public void nextScene() {
        currentScene++;

        // SCENE 1 -----------------------------------------------------
        if (currentScene == 1)
        {
            fadeAnimator.Play("Fadeout");
            
            string[] lines = {
                "Okay class, for the last question",
                "What forms when you mix Hydrogen and Oxygen?",
                "How about you, [StudentName]?"
            };
            GameObject currentTeacher = dialogueLoader.loadDialogue("Teacher Mikko", teacherPrefab, lines);
            teacherAnimations = currentTeacher.GetComponent<Animator>();
            teacherAnimations.SetTrigger("Speaks");
        }
        // --------------------------------------------------------------



        // SCENE 2 --------------------------------------------------------
        else if (currentScene == 2) 
        {
            fadeAnimator.Play("Fadein50");
            backButton.interactable = false;
            nextButton.interactable = false;

            TextMeshProUGUI question = Instantiate(questionPrefab, buttonsChoicesContainer);
            question.text = "What forms when you mix Hydrogen and Oxygen?";

            string[] choices = { "Water", "Rubber", "Iron" };
            for (int i = 0; i < 3; i++)
            {
                Debug.Log("Spawning Button: " + i);
                GameObject buttonChoices = Instantiate(buttonPrefab, buttonsChoicesContainer);
                buttonChoices.GetComponentInChildren<TMPro.TMP_Text>().text = choices[i];
                string currentChoice = choices[i];
                buttonChoices.GetComponent<Button>().onClick.AddListener( () => { choiceButtonClicked(currentChoice);} );
            }
            
            void choiceButtonClicked(string choice)
            {
                backButton.interactable = true;
                nextButton.interactable = true;
                string[] lines;
                if (choice == "Water")
                {
                    lines = new string[] {
                    "Correct!",
                    "When you mix 2 Hydrogen molecules and 1 Hydrogen molecule...",
                    "You get... well, Water!"
                    };
                }
                else
                {
                    lines = new string[] {
                    "Nice try! But unfortunately your answer is wrong!",
                    "When you mix 2 Hydrogen molecules and 1 Hydrogen molecule...",
                    "You get... well, Water!"
                    };
                }

                Destroy(buttonsChoicesContainer.gameObject);
                fadeAnimator.Play("Fadeout50");
                GameObject currentTeacher = dialogueLoader.loadDialogue("Teacher Mikko", teacherPrefab, lines);
                teacherAnimations = currentTeacher.GetComponent<Animator>();
                teacherAnimations.SetTrigger("Speaks");
            }
        }
        // --------------------------------------------------------------------------------
        else
        {
            Debug.Log("End of mission 1");
        }
    }
}
