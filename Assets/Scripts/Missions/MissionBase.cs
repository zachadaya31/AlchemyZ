using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;

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
        SaveProgress(false); // tells the database this chapter has started
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
    protected void ShowMCQ(string label, string questionText, string[] choices, string correctChoice,
        string[] correctLines, string[] wrongLines, string speaker, GameObject speakerPrefab, Sprite background)
    {
        
        fadeAnimator.Play("Fadein50");
        backButton.interactable = false;
        nextButton.interactable = false;
        dialogueBox.SetActive(false);

        TextMeshProUGUI question = Instantiate(questionPrefab, buttonsChoicesContainer);
        question.text = questionText;

        for (int i = 0; i < choices.Length; i++)
        {
            GameObject buttonChoices = Instantiate(buttonPrefab, buttonsChoicesContainer);
            buttonChoices.GetComponentInChildren<TMPro.TMP_Text>().text = choices[i];
            string currentChoice = choices[i];
            buttonChoices.GetComponent<Button>().onClick.AddListener(() => { choiceButtonClicked(currentChoice); });
        }

        void choiceButtonClicked(string choice)
        {
            
            RecordAnswer(label, questionText, choice, correctChoice, false);

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
    protected void ShowPTQ(string label, string questionText, string[] choices, string correctChoice, string explanation, Sprite background = null)
    {
        if (background == null) background = forestNightBackground;

        fadeAnimator.Play("Fadein50");
        backButton.interactable = false;
        nextButton.interactable = false;
        dialogueBox.SetActive(false);

        TextMeshProUGUI question = Instantiate(questionPrefab, buttonsChoicesContainer);
        question.text = questionText;

        for (int i = 0; i < choices.Length; i++)
        {
            GameObject buttonChoices = Instantiate(elementerPTQButtonPrefab, buttonsChoicesContainer);
            buttonChoices.GetComponentInChildren<TMPro.TMP_Text>().text = choices[i];
            string currentChoice = choices[i];
            buttonChoices.GetComponent<Button>().onClick.AddListener(() => { choiceButtonClicked(currentChoice); });
        }

        void choiceButtonClicked(string choice)
        {
            RecordAnswer(label, questionText, choice, correctChoice, true);

            backButton.interactable = true;
            nextButton.interactable = true;

            // "PTQ 3" -> 3
            int number = int.Parse(label.Substring(label.Length - 1));
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
    // DATABASE (new Supabase project). Answers are held here and sent when the 3rd one comes in.
    // try/catch so bad wifi can't freeze the game on stage.
    // =====================================================================

    private Dictionary<string, int> mcqScores = new Dictionary<string, int>();
    private Dictionary<string, int> ptqScores = new Dictionary<string, int>();

    protected void RecordAnswer(string label, string questionText, string choice, string correctChoice, bool isPTQ)
    {
        var scores = isPTQ ? ptqScores : mcqScores;
        if (scores.ContainsKey(label)) return; // first attempt only (also stops back-button re-answers)

        int score = (choice == correctChoice) ? 1 : 0;
        scores[label] = score;
        string kind = isPTQ ? "post" : "pre";

        // save THIS answer right now
        SaveAnswer(kind, new Dictionary<string, object>
        {
            { "questionLabel", label },
            { "question", questionText },
            { "answer", choice },
            { "score", score }
        });

    }

    private async void SaveAnswer(string kind, Dictionary<string, object> answer)
    {
        int saved = await SupabaseManager.Instance.SubmitChapterAnswers(
            ChapterNumber, kind, new List<Dictionary<string, object>> { answer });
        Debug.Log("[" + kind + "] " + answer["questionLabel"] + " saved rows = " + saved);
    }

    private async void SaveScore(string kind, int correct, int total)
    {
        bool ok = await SupabaseManager.Instance.SubmitChapterScore(ChapterNumber, kind, correct, total);
        Debug.Log("[" + kind + "] score " + correct + "/" + total + " saved = " + ok);
    }

    private async void SaveProgress(bool completed)
    {
        await SupabaseManager.Instance.SaveChapterProgress(ChapterNumber, completed);
    }

    protected async void FinishChapter()
    {
        if (mcqScores.Count == 3)
            await SupabaseManager.Instance.SubmitChapterScore(ChapterNumber, "pre", mcqScores.Values.Sum(), 3);
        if (ptqScores.Count == 3)
            await SupabaseManager.Instance.SubmitChapterScore(ChapterNumber, "post", ptqScores.Values.Sum(), 3);

        await SupabaseManager.Instance.SaveChapterProgress(ChapterNumber, true);

        SceneLoader.loadScene("MainMenu");
    }
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