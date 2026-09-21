//using Unity.Android.Gradle;
using System.Collections;
using TMPro;
//using Unity.Android.Gradle;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Mission1 : MonoBehaviour
{
    public int currentScene = 0; // kung pang ilang scene na ang isang mission
    public static Mission1 Instance; // instance ng mission1 para ma access ng ibang class yung nextScene() method

    public Dialogue dialogueLoader;
    public Animator fadeAnimator;
    public GameObject teacherPrefab;
    public GameObject scientistPrefab;

    [Header("Called on Runtime")]
    public Animator teacherAnimations;
    

    [Header("Buttons Funcationality")]
    public GameObject buttonPrefab;
    public Transform buttonsChoicesContainer;
    public TextMeshProUGUI questionPrefab;
    public Button backButton;
    public Button nextButton;
    public GameObject fullscreenButtonPrefab;

    [Header("Canvas")]
    public Transform canvasContainer;
    public GameObject dialogueObject;

    [Header("Backgrounds")]
    public Sprite classroomPicture;
    public Sprite laboratoryPicture;

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
        currentScene++;

        // SCENE 1 -----------------------------------------------------
        if (currentScene == 1)
        {
            StartCoroutine(waitScene());
            fadeAnimator.Play("Fadeout");

            string[] lines = {
                "Okay class... For the last question",
                "What do you get when you combine 2 Hydrogen atoms and 1 Oxygen atom?",
                "How about you, [StudentName]?"
            };
            GameObject currentTeacher = dialogueLoader.loadDialogue("Teacher Mikko", teacherPrefab, lines, classroomPicture);
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
            question.text = "What do you get when you combine 2 Hydrogen atoms and 1 Oxygen atom?";

            string[] choices = { "Water", "Iron", "Rubber" };
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
                    "When 2 Hydrogen atoms and 1 Oxygen atom is combined...",
                    "You get... well, Water!",
                    "That's all for today's class. Goodbye Everyone!"
                    };
                }
                else
                {
                    lines = new string[] {
                    "Nice try! But unfortunately your answer is wrong!",
                    "When 2 Hydrogen atoms and 1 Oxygen atom is combined...",
                    "You get... well, Water!",
                    "That's all for today's class. Goodbye Everyone!"
                    };
                }

                foreach (Transform child in buttonsChoicesContainer)
                {
                    Destroy(child.gameObject);
                }
                fadeAnimator.Play("Fadeout50");
                GameObject currentTeacher = dialogueLoader.loadDialogue("Teacher Mikko", teacherPrefab, lines, classroomPicture);
                teacherAnimations = currentTeacher.GetComponent<Animator>();
                teacherAnimations.SetTrigger("Speaks");
            }
        }
        // --------------------------------------------------------------------------------




        // SCENE 3-------------------------------------------------------------------------
        else if (currentScene == 3)
        {
            nextButton.enabled = false;
            backButton.enabled = false;
            fadeAnimator.Play("Fadein");
            StartCoroutine(textFade());

            IEnumerator textFade()
            {
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
                btnFs.onClick.AddListener(() => { nextScene(); Destroy(fullscreenButton); nextButton.enabled = true; backButton.enabled = true; });
            }

        }

        // ---------------------------------------------------------------------------------




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
            GameObject currentTeacher = dialogueLoader.loadDialogue("Professor Zach", scientistPrefab, lines, laboratoryPicture);
        }

        //--------------------------
        // SCENE 5 - Elementer AR load
        //--------------------------

        else if (currentScene == 5) {
            dialogueObject.SetActive(false);
            SceneManager.LoadScene("AR_Test", LoadSceneMode.Additive);
        }

        //--------------------------
        // SCENE 6 - Picked up Elementer
        //--------------------------    

        else if (currentScene == 6) {
            dialogueObject.SetActive(true);

            fadeAnimator.Play("Fadeout");

            string[] lines = {
                "Great! that device is called the Elementer!",
                "It can spawn elements from the Periodic Table with just a press of a button!",
                "Try it out! Try spawning one Oxygen Element",
            };
            GameObject currentTeacher = dialogueLoader.loadDialogue("Professor Zach", scientistPrefab, lines, laboratoryPicture);
        }

        else
        {
            Debug.Log("End of mission 1");
        }
    }
}
