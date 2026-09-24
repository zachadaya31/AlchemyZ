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
    public GameObject emptyPrefab;
    public GameObject elementerPrefab;

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
    public GameObject dialogueBox;
    public GameObject backgroundAnchor;

    [Header("Video")]
    public GameObject videoPlayerObject;
    public GameObject videoPlayerObject2;
    public UnityEngine.Video.VideoPlayer videoPlayer;
    public UnityEngine.Video.VideoPlayer videoPlayer2;
    

    [Header("Backgrounds")]
    public Sprite classroomPicture;
    public Sprite laboratoryPicture;
    public Sprite forestBackground;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        Instance = this;
        videoPlayerObject.SetActive(false);
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
            dialogueBox.SetActive(false);

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
                dialogueBox.SetActive(true);
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
                question.text = "As your class ends, Professor Zach texted you to immediately come to the laboratory...";
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
                "Ah there it is!",
                "Allow me to introduce to you... the Elementer!",
                "This little machine can can combine elements to create anything!",
                "Water, Medicine, anything you can think of!",
                "But first, you must learn its basics.,,",
                "Tell me apprentice, which symbol in the Periodic Table represents Hydrogen?"
            };
            GameObject currentTeacher = dialogueLoader.loadDialogue("Professor Zach", scientistPrefab, lines, laboratoryPicture);
        }

        else if (currentScene == 7)
        {
            fadeAnimator.Play("Fadein50");
            backButton.interactable = false;
            nextButton.interactable = false;
            dialogueBox.SetActive(false);

            TextMeshProUGUI question = Instantiate(questionPrefab, buttonsChoicesContainer);
            question.text = "Which symbol in the Periodic Table represents Hydrogen?";

            string[] choices = { "Hd", "H", "Hy" };
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
                if (choice == "H")
                {
                    lines = new string[] {
                        "Correct!",
                        "The symbol that represents Hydrogen is H!",
                        "Now, watch closely as the Elementer spawns a Hydrogen element",
                    };
                }
                else
                {
                    lines = new string[] {
                        "Nope, your answer is wrong!",
                        "The symbol that represents Hydrogen is H!",
                        "Now, watch closely as the Elementer spawns a Hydrogen element"
                    };
                }

                foreach (Transform child in buttonsChoicesContainer)
                {
                    Destroy(child.gameObject);
                }
                dialogueBox.SetActive(true);
                fadeAnimator.Play("Fadeout50");
                GameObject currentTeacher = dialogueLoader.loadDialogue("Professor Zach", scientistPrefab, lines, laboratoryPicture);
                teacherAnimations = currentTeacher.GetComponent<Animator>();
                teacherAnimations.SetTrigger("Speaks");
            }
        }

        //--------------------------
        // SCENE 8 - Element spawn video
        //--------------------------

        else if (currentScene == 8)
        {
            dialogueObject.SetActive(false);
            backButton.interactable = false;
            nextButton.interactable = false;

            videoPlayerObject.SetActive(true);
            videoPlayer.Play();
            videoPlayer.loopPointReached += OnVideoFinished;
        }

        //--------------------------
        // SCENE 9 - NASA FOREST NA YUNG STUDENT
        //--------------------------
        else if (currentScene == 9)
        {
                
                videoPlayerObject.SetActive(false);
                foreach (Transform child in buttonsChoicesContainer) {
                    Destroy(child.gameObject);
                }

                fadeAnimator.Play("Fadeout");

                string[] lines = {
                    "Where am I?",
                    "...",
                    "I think i'm gonna pass out...",
                    "I need some water..."
                };
                GameObject currentTeacher = dialogueLoader.loadDialogue("[Student Name]", emptyPrefab, lines, forestBackground);
                backgroundAnchor.transform.position = new Vector3(-4.28f, -0.65f, -1.75f);
                
        }

        else if (currentScene == 10)
        {
            dialogueObject.SetActive(false);
            backButton.interactable = false;
            nextButton.interactable = false;

            videoPlayerObject2.SetActive(true);
            videoPlayer2.Play();
            videoPlayer2.loopPointReached += OnVideoFinished;
        }

        else if (currentScene == 11)
        {
                
                videoPlayerObject.SetActive(false);
                foreach (Transform child in buttonsChoicesContainer) {
                    Destroy(child.gameObject);
                }

                fadeAnimator.Play("Fadeout");

                string[] lines = {
                    "...",
                    "Is this the Elementer..?",
                    "I'll try to make some water to drink.",
                    "Let's give this a shot"
                };
                GameObject currentTeacher = dialogueLoader.loadDialogue("[Student Name]", elementerPrefab, lines, forestBackground);
                backgroundAnchor.transform.position = new Vector3(-4.28f, -0.65f, -1.75f);
                
        }

        else if (currentScene == 12) {
            dialogueObject.SetActive(false);
            SceneManager.LoadScene("Elementer", LoadSceneMode.Additive);
        }

        else
        {
            Debug.Log("End of mission 1");
        }
    }

    void OnVideoFinished(UnityEngine.Video.VideoPlayer vp)
    {
        vp.loopPointReached -= OnVideoFinished;
        videoPlayerObject.SetActive(false);
        videoPlayerObject2.SetActive(false);
        dialogueObject.SetActive(true);
        backButton.interactable = true;
        nextButton.interactable = true;
        nextScene();
    }
}
