//using Unity.Android.Gradle;
using System;
using System.Collections;
using TMPro;

//using Unity.Android.Gradle;
using Unity.VisualScripting;
//using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Mission1 : MonoBehaviour
{
    public int currentScene = 0; // kung pang ilang scene na ang isang mission
    public static Mission1 Instance; // instance ng mission1 para ma access ng ibang class yung nextScene() method
    public bool missionCombinationComplete = false;

    public Dialogue dialogueLoader;
    public Animator fadeAnimator;
    public GameObject teacherPrefab;
    public GameObject scientistPrefab;
    public GameObject emptyPrefab;
    public GameObject elementerPrefab;
    public GameObject elementerAIPrefab;

    [Header("Called on Runtime")]
    public Animator teacherAnimations;
    public GameObject missionCamera;
    public GameObject missionEventSystem;
    

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
    public GameObject characterAnchor;

    [Header("Video")]
    public GameObject videoPlayerObject;
    public GameObject videoPlayerObject2;
    public GameObject videoPlayerObject3;
    public UnityEngine.Video.VideoPlayer videoPlayer;
    public UnityEngine.Video.VideoPlayer videoPlayer2;
    public UnityEngine.Video.VideoPlayer videoPlayer3;
    

    [Header("Backgrounds")]
    public Sprite classroomPicture;
    public Sprite laboratoryPicture;
    public Sprite forestBackground;
    public Sprite forestNightBackground;

    [Header("Instructions")]
    public GameObject instructionsBox;
    public TextMeshProUGUI instructionsText;

    [Header("Student Info")]    
    public string studentFirstName;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        Instance = this;
        videoPlayerObject.SetActive(false);
        videoPlayerObject2.SetActive(false);
        videoPlayerObject3.SetActive(false);
    }
    void Start()
    {
        studentFirstName = SessionManager.Instance.studentFirstName;
        nextScene();
    }

    public async void nextScene() {
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
                "How about you, "+ studentFirstName + "?"
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
            String questionText = question.text = "What do you get when you combine 2 Hydrogen atoms and 1 Oxygen atom?";

            var recordAnswer = await SupabaseManager.Instance.supabase
            .From<Chapter_Answers_Record>()
            .Where(x => x.studentID == SessionManager.Instance.studentID)
            .Where(x => x.Question == questionText)
            .Get();

            string[] choices = { "Water", "Iron", "Rubber" };
            string correctChoice = "Water";
            for (int i = 0; i < 3; i++)
            {
                GameObject buttonChoices = Instantiate(buttonPrefab, buttonsChoicesContainer);
                buttonChoices.GetComponentInChildren<TMPro.TMP_Text>().text = choices[i];
                string currentChoice = choices[i];
                buttonChoices.GetComponent<Button>().onClick.AddListener(() => { choiceButtonClicked(currentChoice); });
            }

            async void choiceButtonClicked(string choice)
            {
                if (recordAnswer.Models.Count > 0)
                {
                    Debug.Log("student already answered");
                }
                else
                {
                    Debug.Log("no student answer. recording answer");
                    int earnedScore = (choice == correctChoice) ? 1 : 0;
                    var newRecord = new Chapter_Answers_Record
                    {
                        chapterID = 1,
                        MCQ_PCQ = "MCQ 1",
                        
                        studentID = SessionManager.Instance.studentID,
                        Question = questionText,
                        Answer = choice,
                        Score = earnedScore
                    };
                    await SupabaseManager.Instance.supabase
                        .From<Chapter_Answers_Record>()
                        .Insert(newRecord);
                }

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
            //SceneManager.LoadScene("AR_Test", LoadSceneMode.Additive);
            StartCoroutine(LoadARScene("AR_Test"));
            missionCamera.SetActive(true);
        }
        

        //--------------------------
        // SCENE 6 - Picked up Elementer
        //--------------------------    

        else if (currentScene == 6) {
            missionCamera.SetActive(true);
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
            string questionText = question.text = "Which symbol in the Periodic Table represents Hydrogen?";

            var recordAnswer = await SupabaseManager.Instance.supabase
            .From<Chapter_Answers_Record>()
            .Where(x => x.studentID == SessionManager.Instance.studentID)
            .Where(x => x.Question == questionText)
            .Get();

            string[] choices = { "Hd", "H", "Hy" };
            string correctChoice = "H";
            for (int i = 0; i < 3; i++)
            {
                GameObject buttonChoices = Instantiate(buttonPrefab, buttonsChoicesContainer);
                buttonChoices.GetComponentInChildren<TMPro.TMP_Text>().text = choices[i];
                string currentChoice = choices[i];
                buttonChoices.GetComponent<Button>().onClick.AddListener(() => { choiceButtonClicked(currentChoice); });
            }

            async void choiceButtonClicked(string choice)
            {
   
                if (recordAnswer.Models.Count > 0)
                {
                    Debug.Log("student already answered");
                }
                else
                {
                    Debug.Log("no student answer. recording answer");
                    int earnedScore = (choice == correctChoice) ? 1 : 0;
                    var newRecord = new Chapter_Answers_Record
                    {
                        chapterID = 1,
                        MCQ_PCQ = "MCQ 1",
                        
                        studentID = SessionManager.Instance.studentID,
                        Question = questionText,
                        Answer = choice,
                        Score = earnedScore
                    };
                    await SupabaseManager.Instance.supabase
                        .From<Chapter_Answers_Record>()
                        .Insert(newRecord);
                }

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
                GameObject currentTeacher = dialogueLoader.loadDialogue(studentFirstName, emptyPrefab, lines, forestBackground);
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
                GameObject currentTeacher = dialogueLoader.loadDialogue(studentFirstName, elementerPrefab, lines, forestBackground);
                backgroundAnchor.transform.position = new Vector3(-4.28f, -0.65f, -1.75f);
                
        }


        //AR ELEMENTER SCENE
        else if (currentScene == 12) {

            dialogueObject.SetActive(false);
            SceneManager.LoadScene("Elementer", LoadSceneMode.Additive);
            //FindAnyObjectByType<ARBackgroundToggle>().SetBackgroundVisible(false);
            missionCamera.SetActive(false);
            missionEventSystem.SetActive(false);

        }

        else if (currentScene == 13)
        {      
            missionCamera.SetActive(true);
            missionEventSystem.SetActive(true);

            _ = SceneManager.UnloadSceneAsync("Elementer");

            dialogueObject.SetActive(true);
            videoPlayerObject.SetActive(false);
            foreach (Transform child in buttonsChoicesContainer)
            {
                Destroy(child.gameObject);
            }

            fadeAnimator.Play("Fadeout");

            string[] lines = {
                    "It worked!",
                    "I just made water using the Elementer!!!",
                    "Ok... I have a lot of questions as of now but I think it's best if I hit the hay for tonight.",
                };
            GameObject currentTeacher = dialogueLoader.loadDialogue(studentFirstName, elementerPrefab, lines, forestBackground);
            backgroundAnchor.transform.position = new Vector3(-4.28f, -0.65f, -1.75f);

        }

        // OUTRO SCNEEEEEEEEEE

        else if (currentScene == 14)
        {
            //SceneManager.UnloadSceneAsync("Elementer");
            dialogueObject.SetActive(false);
            nextButton.enabled = false;
            backButton.enabled = false;

            missionCamera.SetActive(true);
            missionEventSystem.SetActive(true);

            fadeAnimator.Play("Fadein");
            StartCoroutine(textFade());

            IEnumerator textFade()
            {
                TextMeshProUGUI question = Instantiate(questionPrefab, buttonsChoicesContainer);
                Animator textAnimations = question.gameObject.GetComponent<Animator>();
                question.text = "Chapter 1: The Alchemist's Apprentice\nCreate Water | Complete";
                textAnimations.Play("TextFadeinNew");

                yield return new WaitForSeconds(3);

                TextMeshProUGUI continueText = Instantiate(questionPrefab, buttonsChoicesContainer);
                Animator textAnimations2 = continueText.gameObject.GetComponent<Animator>();
                continueText.text = "\n\nPress the screen to go back to the main menu...";
                continueText.fontSize = 50;
                textAnimations2.Play("TextFadeinNew");

                yield return new WaitForSeconds(1);

                GameObject fullscreenButton = Instantiate(fullscreenButtonPrefab, canvasContainer);
                Button btnFs = fullscreenButton.GetComponent<Button>();
                btnFs.onClick.AddListener(() => { nextScene(); Destroy(fullscreenButton); nextButton.enabled = true; backButton.enabled = true; });
            }

        }


        else if (currentScene == 15)
        {
            dialogueObject.SetActive(true);
            videoPlayerObject.SetActive(false);
            foreach (Transform child in buttonsChoicesContainer)
            {
                Destroy(child.gameObject);
            }

            fadeAnimator.Play("Fadeout");

            string[] lines = {
                    "..?",
                    "Huh?",
                };
            GameObject currentTeacher = dialogueLoader.loadDialogue(studentFirstName, emptyPrefab, lines, forestNightBackground);
            backgroundAnchor.transform.position = new Vector3(-4.28f, -0.65f, -1.75f);

        }

        // forst night scene
        else if (currentScene == 16)
        {
            foreach (Transform child in buttonsChoicesContainer) {
                Destroy(child.gameObject);
            }
            fadeAnimator.Play("Fadeout");

            dialogueObject.SetActive(false);
            backButton.interactable = false;
            nextButton.interactable = false;

            videoPlayerObject3.SetActive(true);
            videoPlayer3.isLooping = false;
            videoPlayer3.prepareCompleted += (vp) => vp.Play();
            videoPlayer3.loopPointReached += OnVideoFinished;
            videoPlayer3.Prepare();
        }

        else if (currentScene == 17)
        {
            dialogueObject.SetActive(true);
            videoPlayerObject.SetActive(false);
            foreach (Transform child in buttonsChoicesContainer)
            {
                Destroy(child.gameObject);
            }

            fadeAnimator.Play("Fadeout");

            string[] lines = {
                "Professor Zach?!",
                "Is that you?!",
            };

            GameObject currentTeacher = dialogueLoader.loadDialogue(studentFirstName, emptyPrefab, lines, forestNightBackground);
            
            backgroundAnchor.transform.position = new Vector3(-4.28f, -0.65f, -1.75f);
        }

        else if (currentScene == 18)
        {
            dialogueObject.SetActive(true);
            videoPlayerObject.SetActive(false);
            foreach (Transform child in buttonsChoicesContainer)
            {
                Destroy(child.gameObject);
            }

            fadeAnimator.Play("Fadeout");

            string[] lines = {
                studentFirstName + ", my apprentice!",
                "I'm not exactly Professor Zach...",
                "I'm an AI built into the elementer by Prof. Zach!!!",
                "It seems that the Elementer overloaded and it accidentally opened up an interdimensional portal!",
                "Based on my calculations, you and Professor Zach was put into different dimensions.",
            };

            GameObject currentTeacher = dialogueLoader.loadDialogue("Professor Zach AI", elementerAIPrefab, lines, forestNightBackground);
            
            backgroundAnchor.transform.position = new Vector3(-4.28f, -0.65f, -1.75f);
        }

        else if (currentScene == 19)
        {
            dialogueObject.SetActive(true);
            videoPlayerObject.SetActive(false);
            foreach (Transform child in buttonsChoicesContainer)
            {
                Destroy(child.gameObject);
            }

            fadeAnimator.Play("Fadeout");

            string[] lines =
            {
                "WHAT?!",
                "Another dimension?! Interdimensional portals?!",
                "I don't get any of this...!"
            };

            GameObject currentTeacher = dialogueLoader.loadDialogue(studentFirstName, emptyPrefab, lines, forestNightBackground);
            
            backgroundAnchor.transform.position = new Vector3(-4.28f, -0.65f, -1.75f);
        }

        else if (currentScene == 20)
        {
            dialogueObject.SetActive(true);
            videoPlayerObject.SetActive(false);
            foreach (Transform child in buttonsChoicesContainer)
            {
                Destroy(child.gameObject);
            }

            fadeAnimator.Play("Fadeout");

            string[] lines =
            {
                "I know this is a lot to uncover and this might overwhelm you.",
                "This is why Professor Zach built an AI like me!",
                "Don't worry I'll get us out of this",
                "But first, because of the overloading, my database seems to be miscalibrated...",
                "I need you to answer these questions just to refresh the Elementer's Information Databse",
                "Let's start."
            };

            GameObject currentTeacher = dialogueLoader.loadDialogue("Professor Zach AI", elementerAIPrefab, lines, forestNightBackground);
            
            backgroundAnchor.transform.position = new Vector3(-4.28f, -0.65f, -1.75f);
        }

        else
        {
            Debug.Log("End of mission 1");
            SceneManager.LoadScene("MainMenu");

        }

        
    }

void OnVideoFinished(UnityEngine.Video.VideoPlayer vp)
{
    vp.loopPointReached -= OnVideoFinished;
    
    videoPlayerObject.SetActive(false);
    videoPlayerObject2.SetActive(false);
    videoPlayerObject3.SetActive(false);
    
    dialogueObject.SetActive(true);
    backButton.interactable = true;
    nextButton.interactable = true;
    nextScene();
}

    private IEnumerator LoadARScene(string AR_SCENE_NAME)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(AR_SCENE_NAME, LoadSceneMode.Additive);
        
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        yield return new WaitForEndOfFrame();

        HideSimulatedEnvironment();
    }

    public static void HideSimulatedEnvironment()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            
            if (scene.name == "AR_Test")
            {
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    if (root.name.StartsWith("Simulated Environment Scene"))
                    {
                        // Instead of moving the root (which XR Simulation overrides),
                        // move all its children up together!
                        foreach (Transform child in root.transform)
                        {
                            child.position += new Vector3(0, 5f, 0);
                        }
                        
                        Debug.Log("Successfully shifted simulated environment contents up!");
                    }
                }
            }
        }
    }
}
