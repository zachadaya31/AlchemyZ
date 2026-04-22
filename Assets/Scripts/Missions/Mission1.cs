//using Unity.Android.Gradle;
using System.Collections;
using TMPro;
//using Unity.Android.Gradle;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Mission1 : MonoBehaviour
{
    private int currentScene = 0;
    public static Mission1 Instance;

    public SoundManager soundManager;

    public Dialogue dialogueLoader;
    public Animator fadeAnimator;
    public GameObject teacherPrefab;
    public GameObject scientistPrefab;

    [Header("Called on Runtime")]
    public Animator teacherAnimations;
    

    [Header("Buttons")]
    public GameObject buttonPrefab;
    public Transform buttonsChoicesContainer;
    public TextMeshProUGUI questionPrefab;
    public Button backButton;
    public Button nextButton;
    public GameObject fullscreenButtonPrefab;

    [Header("Canvas")]
    public Transform canvasContainer;


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
        IEnumerator waitScene()
        {
            nextButton.interactable = false;
            backButton.interactable = false;
            yield return new WaitForSeconds(1.5f);
            nextButton.interactable = true;
            backButton.interactable = true;
        }
        StartCoroutine(waitScene());
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
        // --------------------------------------------------------------
        // --------------------------------------------------------------
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
                GameObject buttonChoices = Instantiate(buttonPrefab, buttonsChoicesContainer);
                buttonChoices.GetComponentInChildren<TMPro.TMP_Text>().text = choices[i];
                string currentChoice = choices[i];
                buttonChoices.GetComponent<Button>().onClick.AddListener(() => { choiceButtonClicked(currentChoice); });
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
                    "When you mix 2 Hydrogen molecules and 1 Oxygen molecule...",
                    "You get... well, Water!",
                    "That's all for today's class. Goodbye Everyone!"
                    };
                }
                else
                {
                    lines = new string[] {
                    "Nice try! But unfortunately your answer is wrong!",
                    "When you mix 2 Hydrogen molecules and 1 Oxygen molecule...",
                    "You get... well, Water!",
                    "That's all for today's class. Goodbye Everyone!"
                    };
                }

                foreach (Transform child in buttonsChoicesContainer)
                {
                    Destroy(child.gameObject);
                }
                fadeAnimator.Play("Fadeout50");
                GameObject currentTeacher = dialogueLoader.loadDialogue("Teacher Mikko", teacherPrefab, lines);
                teacherAnimations = currentTeacher.GetComponent<Animator>();
                teacherAnimations.SetTrigger("Speaks");
            }
        }
        // --------------------------------------------------------------------------------
        // --------------------------------------------------------------
        // --------------------------------------------------------------
        // --------------------------------------------------------------


        // SCENE 3-------------------------------------------------------------------------
        else if (currentScene == 3)
        {
            nextButton.enabled = false;
            backButton.enabled = false;
            fadeAnimator.Play("Fadein");
            StartCoroutine(textFade());

            IEnumerator textFade()
            {
                soundManager.playTextSound();
                yield return new WaitForSeconds(0.5f);
                soundManager.playTextSound();
                yield return new WaitForSeconds(0.5f);
                soundManager.playTextSound();
                yield return new WaitForSeconds(0.5f);
                soundManager.playTextSound();
                yield return new WaitForSeconds(0.5f);

                TextMeshProUGUI question = Instantiate(questionPrefab, buttonsChoicesContainer);
                Animator textAnimations = question.gameObject.GetComponent<Animator>();
                question.text = "As your class ends, Professor Wally texted you to immediately come to the laboratory...";
                textAnimations.Play("TextFadeinNew");

                yield return new WaitForSeconds(3);

                TextMeshProUGUI continueText = Instantiate(questionPrefab, buttonsChoicesContainer);
                Animator textAnimations2 = continueText.gameObject.GetComponent<Animator>();
                continueText.text = "\n\nPress the screen to continue...";
                continueText.fontSize = 50;
                textAnimations2.Play("TextFadeinNew");

                yield return new WaitForSeconds(1);

                GameObject fullscreenButton = Instantiate(fullscreenButtonPrefab, canvasContainer);
                Button btnFs = fullscreenButton.GetComponent<Button>();
                btnFs.onClick.AddListener(() => { nextScene(); Destroy(fullscreenButton); nextButton.enabled = true; backButton.enabled = true; } );
            }
            
        }

        // ---------------------------------------------------------------------------------
        // --------------------------------------------------------------------------------
        // --------------------------------------------------------------
        // --------------------------------------------------------------
        // --------------------------------------------------------------

        // SCENE 4 ---------------------------------------------------------------------------
        else if (currentScene == 4)
        {
            foreach (Transform child in buttonsChoicesContainer) {
                Destroy(child.gameObject);
            }

            fadeAnimator.Play("Fadeout");

            string[] lines = {
                "My apprentice!",
                "Come! I must show you something!",
                "I've already finished the `Prototype` !!!",
                "Quick, look around and grab it for me!"
            };
            GameObject currentTeacher = dialogueLoader.loadDialogue("Professor Wally", scientistPrefab, lines);
        }
        else
        {
            Debug.Log("End of mission 1");
        }
    }
}
