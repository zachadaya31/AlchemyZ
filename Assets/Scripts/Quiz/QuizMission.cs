using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Which quiz the student tapped in the Quiz list scene.
public static class QuizSelection
{
    public static string id;
    public static int number;
    public static string title;
    public static string material;
}

// Quiz run as a story scene: Clarence AI talks to the student, then PTQ-style questions (4 choices).
// Use in a scene duplicated from a Mission scene, in place of MissionN.
public class QuizMission : MissionBase
{
    public override int ChapterNumber => 0;   // not a chapter, so no chapter progress is saved

    [Header("Quiz only")]
    public Sprite quizBackground;             // empty = forest night (same as the PTQs)
    public string scoreKind = "post";         // quiz_scores accepts "pre" or "post"
    public string listScene = "Quiz";

    private class Choice { public string text; public bool correct; }
    private class Question { public string text; public List<Choice> choices = new List<Choice>(); }

    private List<Action> steps;
    private List<Question> questions = new List<Question>();
    private Task loadTask;
    private bool loaded, hasTimeLimit, timed, finished;
    private int timeLimitMinutes, correctCount, resultStep;
    private float endTime;
    private string loadError = "";

    private Sprite Bg { get { return quizBackground != null ? quizBackground : forestNightBackground; } }
    private void AiSays(params string[] lines) { Say("Clarence AI", elementerAIPrefab, Bg, lines); }

    protected override void Awake()
    {
        base.Awake();
        string name = string.IsNullOrEmpty(studentFirstName) && SessionManager.Instance != null
            ? SessionManager.Instance.studentFirstName : studentFirstName;
        string quizName = string.IsNullOrEmpty(QuizSelection.title) ? "Quiz" : QuizSelection.title;

        steps = new List<Action>
        {
            () => { StartCoroutine(WaitScene()); AiSays("Welcome, " + name + "!", "I am Clarence AI. Today's assessment is: " + quizName + "."); },
            () => AiSays(string.IsNullOrEmpty(QuizSelection.material)
                    ? new string[] { "Before we begin, have you already reviewed your lessons?" }
                    : new string[] { "Before we begin, have you already reviewed the lesson?", "This quiz is based on: " + QuizSelection.material }),
            () => AskReady(),
        };
    }

    protected override void Start()
    {
        studentFirstName = SessionManager.Instance != null ? SessionManager.Instance.studentFirstName : "Student";
        loadTask = LoadQuiz();   // loads while the AI is talking
        nextScene();
    }

    public override void nextScene()
    {
        currentScene++;
        if (currentScene <= steps.Count) steps[currentScene - 1]();
        else SceneLoader.loadScene(listScene);
    }

    // ---------------- LOAD (server shuffles questions + applies questions per attempt) ----------------
    private async Task LoadQuiz()
    {
        try
        {
            float t = 0f;
            while (SupabaseManager.Instance.supabase == null && t < 5f) { await Task.Yield(); t += Time.unscaledDeltaTime; }

            var res = await SupabaseManager.Instance.supabase.Rpc("get_quiz",
                new Dictionary<string, object> { { "quizId", QuizSelection.id } });
            JObject data = JObject.Parse(res.Content);

            JToken s = data["settings"];
            bool shuffleChoices = (bool)s["shuffleChoices"];
            hasTimeLimit = (bool)s["timeLimitEnabled"];
            timeLimitMinutes = (int)s["timeLimit"];

            foreach (JToken q in data["questions"])
            {
                var question = new Question { text = Fmt((string)q["questionText"]) };
                string[] texts = { Fmt((string)q["choiceA"]), Fmt((string)q["choiceB"]), Fmt((string)q["choiceC"]), Fmt((string)q["choiceD"]) };
                int correctIndex = (int)q["correctIndex"];
                for (int i = 0; i < texts.Length; i++)
                    question.choices.Add(new Choice { text = texts[i], correct = i == correctIndex });
                if (shuffleChoices) Shuffle(question.choices);
                questions.Add(question);
            }
            loaded = questions.Count > 0;
        }
        catch (Exception e)
        {
            Debug.LogWarning("get_quiz failed: " + e.Message);
            loadError = e.Message.Contains("QUIZ_CLOSED") ? "This quiz is closed right now."
                      : e.Message.Contains("NO_QUESTIONS") ? "This quiz has no questions yet."
                      : "I could not load the quiz. Please check your internet.";
        }
    }

    // ---------------- "ARE YOU READY?" ----------------
    private void AskReady()
    {
        fadeAnimator.Play("Fadein50");
        backButton.interactable = false;
        nextButton.interactable = false;
        dialogueBox.SetActive(false);

        Instantiate(questionPrefab, buttonsChoicesContainer).text = "Are you ready to start the quiz?";
        AddChoice("Yes, I'm ready!", OnReady);
        AddChoice("Not yet, I'll review first", OnNotYet);
    }

    private void OnNotYet()
    {
        ClearChoices();
        nextButton.interactable = true;
        currentScene = steps.Count;   // the next tap goes back to the quiz list
        AiSays("Of course. Take your time and review.", "Come back when you are ready!");
    }

    private async void OnReady()
    {
        ClearChoices();
        Instantiate(questionPrefab, buttonsChoicesContainer).text = "Preparing your quiz...";
        await loadTask;
        if (this == null) return;
        ClearChoices();

        if (!loaded)
        {
            nextButton.interactable = true;
            currentScene = steps.Count;
            AiSays(string.IsNullOrEmpty(loadError) ? "I could not load the quiz." : loadError);
            return;
        }

        for (int i = 0; i < questions.Count; i++)
        {
            int idx = i;
            steps.Add(() => ShowQuestion(idx));
        }
        resultStep = steps.Count + 1;
        steps.Add(() => ShowResult());
        steps.Add(() => ShowTextCard("Quiz Complete\n" + QuizSelection.title,
                                     "\n\nPress the screen to go back to the quizzes..."));

        timed = hasTimeLimit;
        endTime = Time.time + timeLimitMinutes * 60f;
        AiSays("Alright! Here we go. Good luck!");   // next tap starts question 1
        nextButton.interactable = true;
    }

    // ---------------- QUESTION (copy of ShowPTQ with 4 choices) ----------------
    private void ShowQuestion(int idx)
    {
        Question q = questions[idx];

        fadeAnimator.Play("Fadein50");
        backButton.interactable = false;
        nextButton.interactable = false;
        dialogueBox.SetActive(false);
        if (timed && instructionsBox != null) instructionsBox.SetActive(true);

        Instantiate(questionPrefab, buttonsChoicesContainer).text = q.text;

        foreach (Choice c in q.choices)
        {
            Choice choice = c;
            AddChoice(choice.text, () => Answered(idx, choice));
        }
    }

    private void Answered(int idx, Choice picked)
    {
        if (finished) return;
        if (picked.correct) correctCount++;

        Question q = questions[idx];
        var lines = new List<string>();
        if (picked.correct) lines.Add("Correct!");
        else
        {
            lines.Add("Nice try! But unfortunately your answer is wrong!");
            lines.Add("The correct answer is: " + q.choices.Find(x => x.correct).text);
        }
        lines.Add((idx + 1) + "/" + questions.Count + " questions answered.");

        ClearChoices();
        nextButton.interactable = true;   // back stays off so answers can't be redone
        dialogueBox.SetActive(true);
        fadeAnimator.Play("Fadeout50");
        GameObject current = dialogueLoader.loadDialogue("Clarence AI", elementerAIPrefab, lines.ToArray(), Bg);
        TriggerSpeak(current);
    }

    // ---------------- TIMER ----------------
    void Update()
    {
        if (!timed || finished) return;
        float left = endTime - Time.time;
        if (left <= 0f) { TimeUp(); return; }
        if (instructionsText != null)
            instructionsText.text = "Time left: " + Mathf.FloorToInt(left / 60f) + ":" + (Mathf.FloorToInt(left) % 60).ToString("00");
    }

    private void TimeUp()
    {
        timed = false;
        currentScene = resultStep - 1;   // jump to the result, unanswered questions count as wrong
        nextScene();
    }

    // ---------------- RESULT ----------------
    private async void ShowResult()
    {
        if (finished) return;
        finished = true;
        timed = false;
        if (instructionsBox != null) instructionsBox.SetActive(false);

        backButton.interactable = false;
        nextButton.interactable = false;
        ClearChoices();
        dialogueBox.SetActive(false);
        Instantiate(questionPrefab, buttonsChoicesContainer).text = "Calculating your score...";

        int total = questions.Count;
        string[] lines;
        try
        {
            var res = await SupabaseManager.Instance.supabase.Rpc("submit_quiz_score",
                new Dictionary<string, object>
                {
                    { "quizId", QuizSelection.id },
                    { "kind", scoreKind },
                    { "correct", correctCount },
                    { "total", total }
                });
            JObject r = JObject.Parse(res.Content);
            bool passed = (bool)r["passed"];
            lines = new string[]
            {
                "Quiz complete!",
                "Score: " + correctCount + "/" + total + " (" + (int)r["score"] + "%)",
                passed ? "You passed! Well done!" : "The passing score is " + (int)r["passingScore"] + "%. Review the lesson and try again!"
            };
        }
        catch (Exception e)
        {
            Debug.LogError("Could not save quiz score: " + e);
            lines = new string[] { "Quiz complete!", "Score: " + correctCount + "/" + total, "But I could not save your score. Please check your internet." };
        }

        if (this == null) return;
        ClearChoices();
        nextButton.interactable = true;
        AiSays(lines);
    }

    // ---------------- helpers ----------------
    private void AddChoice(string label, UnityAction onClick)
    {
        GameObject b = Instantiate(elementerPTQButtonPrefab, buttonsChoicesContainer);
        b.GetComponentInChildren<TMPro.TMP_Text>().text = label;
        b.GetComponent<Button>().onClick.AddListener(onClick);
    }

    // turns unicode superscripts/subscripts (P³⁻, H₂O) into <sup>/<sub> tags, so any font can show them
    private static string Fmt(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        const string sup = "⁰¹²³⁴⁵⁶⁷⁸⁹⁺⁻", norm = "0123456789+-", sub = "₀₁₂₃₄₅₆₇₈₉₊₋";
        var sb = new System.Text.StringBuilder();
        string open = null;
        foreach (char c in s)
        {
            int a = sup.IndexOf(c), b = sub.IndexOf(c);
            string tag = a >= 0 ? "sup" : b >= 0 ? "sub" : null;
            if (tag != open)
            {
                if (open != null) sb.Append("</" + open + ">");
                if (tag != null) sb.Append("<" + tag + ">");
                open = tag;
            }
            sb.Append(a >= 0 ? norm[a] : b >= 0 ? norm[b] : c);
        }
        if (open != null) sb.Append("</" + open + ">");
        return sb.ToString();
    }

    private static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            T tmp = list[i]; list[i] = list[j]; list[j] = tmp;
        }
    }
}