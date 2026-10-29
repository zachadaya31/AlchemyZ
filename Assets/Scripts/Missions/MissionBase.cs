using System;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Everything that every chapter (Mission1, Mission2, ...) shares lives here.
// Each chapter only has to write its own story in nextScene().
public abstract class MissionBase : MonoBehaviour
{
    // ONE instance for ALL chapters. MixManager, DialogueLoader, etc. use MissionBase.Instance.nextScene()
    public static MissionBase Instance;

    // each chapter says which chapter it is (this is what gets saved in the database)
    public abstract int ChapterNumber { get; }

    // what the Elementer scene should wait for in this chapter, ex: "Salt".
    // leave it "" and MixManager just uses its own inspector value (this is how Mission1 still works)
    public virtual string RequiredMixResult => "";

    // the instruction text shown in the Elementer scene for this chapter. "" = keep MixManager's own text
    public virtual string MixInstructions => "";

    // each chapter writes its own story here
    public abstract void nextScene();

    public int currentScene = 0; // kung pang ilang scene na ang isang mission
    public bool missionCombinationComplete = false;

    public Dialogue dialogueLoader;
    // fade in and fade out para sa mga transitions ng game
    public Animator fadeAnimator;

    // teacher mikko character
    public GameObject teacherPrefab;

    //professor zahc character
    public GameObject scientistPrefab;

    //empty prefab for tuwing nagsasalita si student so walang character sa ui since first perosn POV
    public GameObject emptyPrefab;

    // big elementer prefab to show elementer at some moments
    public GameObject elementerPrefab;

    //clarence AI talking to student
    public GameObject elementerAIPrefab;

    [Header("Called on Runtime")]
    public Animator teacherAnimations;
    public GameObject missionCamera;
    public GameObject missionEventSystem;


    [Header("Buttons Funcationality")]
    public GameObject buttonPrefab;
    // buttons for PTQ (blue buttons, elementer themed)
    public GameObject elementerPTQButtonPrefab;
    // yung container ng mga buttons sa PTQ at MCQ
    public Transform buttonsChoicesContainer;
    // buttons for MCQ (yellowish buttons, story themed)
    public TextMeshProUGUI questionPrefab;
    //back button for scens
    public Button backButton;
    //next button for scenes
    public Button nextButton;
    //button sa buong screen for like continue to main menu type screens where its all black
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

    protected virtual void Awake()
    {
        Instance = this;
        videoPlayerObject.SetActive(false);
        videoPlayerObject2.SetActive(false);
        videoPlayerObject3.SetActive(false);
    }

    protected virtual void Start()
    {
        studentFirstName = SessionManager.Instance.studentFirstName;
        nextScene();
    }

    // =====================================================================
    // SMALL HELPERS (new chapters use these so they don't copy-paste 20 lines per scene)
    // =====================================================================

    protected void ClearChoices()
    {
        foreach (Transform child in buttonsChoicesContainer)
        {
            Destroy(child.gameObject);
        }
    }

    // normal story scene: one speaker says a bunch of lines (same stuff as scenes 4, 6, 17-22 in Mission1)
    protected GameObject Say(string speaker, GameObject prefab, Sprite background, params string[] lines)
    {
        dialogueObject.SetActive(true);
        dialogueBox.SetActive(true);
        videoPlayerObject.SetActive(false);
        ClearChoices();

        fadeAnimator.Play("Fadeout");

        GameObject current = dialogueLoader.loadDialogue(speaker, prefab, lines, background);

        // same offset Mission1 uses for the forest backgrounds
        if (background == forestBackground || background == forestNightBackground)
        {
            backgroundAnchor.transform.position = new Vector3(-4.28f, -0.65f, -1.75f);
        }

        TriggerSpeak(current);
        return current;
    }

    // some prefabs (ElementerHologram, EmptyCharacter) have no Animator so we null check
    protected void TriggerSpeak(GameObject current)
    {
        if (current == null) return;
        teacherAnimations = current.GetComponent<Animator>();
        if (teacherAnimations != null)
        {
            teacherAnimations.SetTrigger("Speaks");
        }
    }

    // stops accidental double taps when a scene starts
    protected IEnumerator WaitScene()
    {
        nextButton.interactable = false;
        backButton.interactable = false;
        yield return new WaitForSeconds(1.5f);
        nextButton.interactable = true;
        backButton.interactable = true;
    }

    // black screen with a text + "press the screen to continue" (scenes 3, 16, 26 in Mission1)
    protected void ShowTextCard(string text, string continueLine = "\n\nPress the screen to continue...")
    {
        dialogueObject.SetActive(false);
        nextButton.enabled = false;
        backButton.enabled = false;

        missionCamera.SetActive(true);
        missionEventSystem.SetActive(true);

        fadeAnimator.Play("Fadein");
        StartCoroutine(TextFade(text, continueLine));
    }

    private IEnumerator TextFade(string text, string continueLine)
    {
        TextMeshProUGUI question = Instantiate(questionPrefab, buttonsChoicesContainer);
        Animator textAnimations = question.gameObject.GetComponent<Animator>();
        question.text = text;
        textAnimations.Play("TextFadeinNew");

        yield return new WaitForSeconds(3);

        TextMeshProUGUI continueText = Instantiate(questionPrefab, buttonsChoicesContainer);
        Animator textAnimations2 = continueText.gameObject.GetComponent<Animator>();
        continueText.text = continueLine;
        continueText.fontSize = 50;
        textAnimations2.Play("TextFadeinNew");

        yield return new WaitForSeconds(1);

        GameObject fullscreenButton = Instantiate(fullscreenButtonPrefab, canvasContainer);
        Button btnFs = fullscreenButton.GetComponent<Button>();
        btnFs.onClick.AddListener(() => { nextScene(); Destroy(fullscreenButton); nextButton.enabled = true; backButton.enabled = true; });
    }

    // =====================================================================
    // MCQ (yellow story buttons). Same flow as the MCQ scenes in Mission1, just reusable.
    // =====================================================================
    protected async void ShowMCQ(string label, string questionText, string[] choices, string correctChoice,
        string[] correctLines, string[] wrongLines, string speaker, GameObject speakerPrefab, Sprite background)
    {
        fadeAnimator.Play("Fadein50");
        backButton.interactable = false;
        nextButton.interactable = false;
        dialogueBox.SetActive(false);

        TextMeshProUGUI question = Instantiate(questionPrefab, buttonsChoicesContainer);
        question.text = questionText;

        bool alreadyAnswered = await AlreadyAnswered(questionText);

        for (int i = 0; i < choices.Length; i++)
        {
            GameObject buttonChoices = Instantiate(buttonPrefab, buttonsChoicesContainer);
            buttonChoices.GetComponentInChildren<TMPro.TMP_Text>().text = choices[i];
            string currentChoice = choices[i];
            buttonChoices.GetComponent<Button>().onClick.AddListener(() => { choiceButtonClicked(currentChoice); });
        }

        async void choiceButtonClicked(string choice)
        {
            if (alreadyAnswered)
            {
                Debug.Log("student already answered");
            }
            else
            {
                Debug.Log("no student answer. recording answer");
                await SaveAnswer(label, questionText, choice, correctChoice);
            }

            backButton.interactable = true;
            nextButton.interactable = true;

            string[] lines = (choice == correctChoice) ? correctLines : wrongLines;

            ClearChoices();
            dialogueBox.SetActive(true);
            fadeAnimator.Play("Fadeout50");
            GameObject current = dialogueLoader.loadDialogue(speaker, speakerPrefab, lines, background);
            if (background == forestBackground || background == forestNightBackground)
            {
                backgroundAnchor.transform.position = new Vector3(-4.28f, -0.65f, -1.75f);
            }
            TriggerSpeak(current);
        }
    }

    // =====================================================================
    // PTQ (blue Elementer buttons, Clarence AI recalibration). Same as Mission1's ShowPTQ.
    // background is optional: leave it out and it uses the forest night one like before
    // =====================================================================
    protected async void ShowPTQ(string label, string questionText, string[] choices, string correctChoice, string explanation, Sprite background = null)
    {
        if (background == null) background = forestNightBackground;

        fadeAnimator.Play("Fadein50");
        backButton.interactable = false;
        nextButton.interactable = false;
        dialogueBox.SetActive(false);

        TextMeshProUGUI question = Instantiate(questionPrefab, buttonsChoicesContainer);
        question.text = questionText;

        bool alreadyAnswered = await AlreadyAnswered(questionText);

        for (int i = 0; i < choices.Length; i++)
        {
            GameObject buttonChoices = Instantiate(elementerPTQButtonPrefab, buttonsChoicesContainer);
            buttonChoices.GetComponentInChildren<TMPro.TMP_Text>().text = choices[i];
            string currentChoice = choices[i];
            buttonChoices.GetComponent<Button>().onClick.AddListener(() => { choiceButtonClicked(currentChoice); });
        }

        async void choiceButtonClicked(string choice)
        {
            if (alreadyAnswered)
            {
                Debug.Log("student already answered");
            }
            else
            {
                Debug.Log("no student answer. recording answer");
                await SaveAnswer(label, questionText, choice, correctChoice);
            }

            backButton.interactable = true;
            nextButton.interactable = true;

            // "PTQ 3" -> 3
            int number = int.Parse(label.Replace("PTQ ", ""));
            int percent = Mathf.RoundToInt(number * 100f / 3f);

            string firstLine = (choice == correctChoice) ? "Correct!" : "Nice try! But unfortunately your answer is wrong!";
            string statusLine = (number == 3) ? "3/3 questions answered. Recalibration complete!" : number + "/3 questions answered.";

            string[] lines = new string[]
            {
                firstLine,
                explanation,
                "Elementer recalibration: " + percent + "%",
                statusLine
            };

            ClearChoices();
            dialogueBox.SetActive(true);
            fadeAnimator.Play("Fadeout50");
            GameObject current = dialogueLoader.loadDialogue("Clarence AI", elementerAIPrefab, lines, background);
            TriggerSpeak(current);
        }
    }

    // =====================================================================
    // DATABASE (shared by MCQ and PTQ). try/catch so bad wifi can't freeze the game on stage.
    // =====================================================================
    async Task<bool> AlreadyAnswered(string questionText)
    {
        try
        {
            var recordAnswer = await SupabaseManager.Instance.supabase
                .From<Chapter_Answers_Record>()
                .Where(x => x.studentID == SessionManager.Instance.studentID)
                .Where(x => x.Question == questionText)
                .Get();
            return recordAnswer.Models.Count > 0;
        }
        catch (Exception e)
        {
            Debug.LogError("Could not check old answers: " + e);
            return false;
        }
    }

    async Task SaveAnswer(string label, string questionText, string choice, string correctChoice)
    {
        try
        {
            int earnedScore = (choice == correctChoice) ? 1 : 0;
            var newRecord = new Chapter_Answers_Record
            {
                chapterID = ChapterNumber,
                MCQ_PCQ = "Chapter " + ChapterNumber + " | " + label,   // saved like: Chapter 2 | MCQ 1
                studentID = SessionManager.Instance.studentID,
                Question = questionText,
                Answer = choice,
                Score = earnedScore
            };
            await SupabaseManager.Instance.supabase
                .From<Chapter_Answers_Record>()
                .Insert(newRecord);

            await AddScore(label, earnedScore);
        }
        catch (Exception e)
        {
            Debug.LogError("Could not save answer: " + e);
        }
    }

    protected async Task AddScore(string label, int earnedScore)
    {
        int chapter = ChapterNumber;

        var result = await SupabaseManager.Instance.supabase
            .From<Student_Chapter_Scores>()
            .Where(x => x.studentID == SessionManager.Instance.studentID)
            .Where(x => x.chapterID == chapter)
            .Get();

        bool isMCQ = label.Contains("MCQ"); // works for "MCQ 1" and "Chapter 2 | MCQ 1"

        if (result.Models.Count == 0)
        {
            var newRow = new Student_Chapter_Scores
            {
                studentID = SessionManager.Instance.studentID,
                chapterID = chapter,
                MCQ_Score = isMCQ ? earnedScore : (int?)null,
                PTQ_Score = isMCQ ? (int?)null : earnedScore
            };
            await SupabaseManager.Instance.supabase
                .From<Student_Chapter_Scores>()
                .Insert(newRow);
        }
        else
        {
            var row = result.Models[0];
            if (isMCQ) row.MCQ_Score = (row.MCQ_Score ?? 0) + earnedScore;
            else row.PTQ_Score = (row.PTQ_Score ?? 0) + earnedScore;

            await SupabaseManager.Instance.supabase
                .From<Student_Chapter_Scores>()
                .Update(row);
        }
    }

    // =====================================================================
    // END OF CHAPTER: unlock the next chapter in the database, THEN go back to the main menu
    // (ChapterManager reads studentChapterProgress to decide which chapters are locked)
    // =====================================================================
    protected async void FinishChapter()
    {
        try
        {
            var student = await SupabaseManager.Instance.supabase
                .From<Students>()
                .Where(s => s.studentID == SessionManager.Instance.studentID)
                .Single();

            if (student.studentChapterProgress < ChapterNumber + 1)
            {
                student.studentChapterProgress = ChapterNumber + 1;
                await SupabaseManager.Instance.supabase
                    .From<Students>()
                    .Update(student);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Could not unlock next chapter: " + e);
        }

        SceneLoader.loadScene("MainMenu"); // shows the loading screen
    }

    // =====================================================================
    // VIDEO + AR (moved from Mission1, unchanged)
    // =====================================================================
    protected void OnVideoFinished(UnityEngine.Video.VideoPlayer vp)
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

    protected IEnumerator LoadARScene(string AR_SCENE_NAME)
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