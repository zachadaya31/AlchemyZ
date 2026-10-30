using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Postgrest.Attributes;
using Postgrest.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Table("quizzes")]
public class QuizRow : BaseModel
{
    [PrimaryKey("id")] public string id { get; set; }
    [Column("quizNumber")] public int quizNumber { get; set; }
    [Column("title")] public string title { get; set; }
}

// Quiz LIST scene only. Tapping a quiz opens the "QuizMission" scene (Clarence AI runs the quiz).
public class QuizManager : MonoBehaviour
{
    [Header("Scene")]
    public Transform content;              // Scroll Rect > Content (Vertical Layout Group + Content Size Fitter)
    public TMP_Text headerText;            // optional

    [Header("Prefabs")]
    public GameObject listButtonPrefab;    // a Button with a TMP text child
    public TMP_Text textPrefab;            // for "Loading..." / error messages

    [Header("Edit here. Element 0 = Quiz 1, Element 1 = Quiz 2, ...")]
    public string[] quizTitles;            // empty = database title
    [Tooltip("Lesson file(s) the AI tells the student to review, ex: MOLE-MOLAR-MASS.pdf")]
    public string[] quizMaterials;

    [Header("Scenes")]
    public string quizMissionScene = "QuizMission";
    public string backScene = "MainMenu";

    void Start() { _ = ShowList(); }

    // hook Button - Back's OnClick to this
    public void BackClicked() { SceneLoader.loadScene(backScene); }

    private async Task ShowList()
    {
        if (headerText != null) headerText.text = "Quiz";
        Clear();
        AddText("Loading...");

        float t = 0f;
        while (SupabaseManager.Instance.supabase == null && t < 5f) { await Task.Yield(); t += Time.unscaledDeltaTime; }

        List<QuizRow> rows;
        try
        {
            var res = await SupabaseManager.Instance.supabase.From<QuizRow>().Get();
            rows = res.Models.OrderBy(q => q.quizNumber).ToList();
        }
        catch (Exception e)
        {
            Debug.LogError("Could not load quizzes: " + e);
            if (this == null) return;
            Clear(); AddText("Could not load quizzes. Check your internet.");
            return;
        }

        if (this == null) return;
        Clear();
        if (rows.Count == 0) { AddText("No quizzes yet."); return; }

        foreach (QuizRow row in rows)
        {
            QuizRow r = row;
            string title = Pick(quizTitles, r.quizNumber, r.title);
            string material = Pick(quizMaterials, r.quizNumber, "");

            GameObject go = Instantiate(listButtonPrefab, content, false);
            go.GetComponentInChildren<TMP_Text>().text = title;
            go.GetComponent<Button>().onClick.AddListener(() =>
            {
                QuizSelection.id = r.id;
                QuizSelection.number = r.quizNumber;
                QuizSelection.title = title;
                QuizSelection.material = material;
                SceneLoader.loadScene(quizMissionScene);
            });
        }
    }

    private static string Pick(string[] arr, int quizNumber, string fallback)
    {
        int i = quizNumber - 1;
        if (arr != null && i >= 0 && i < arr.Length && !string.IsNullOrWhiteSpace(arr[i])) return arr[i];
        return fallback;
    }

    private void AddText(string s) { Instantiate(textPrefab, content).text = s; }
    private void Clear() { foreach (Transform c in content) Destroy(c.gameObject); }
}