using UnityEngine;
using Supabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Postgrest.Attributes;
using Postgrest.Models;

public class SupabaseManager : MonoBehaviour
{
    private string url = "https://hbyncudwlokhnthqhylu.supabase.co";
    private string key = "sb_publishable_tgiZ7CQ7XY82pNIxQYWtzg_aeDpqBr-";

    public Client supabase;
    public static SupabaseManager Instance;

    async void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        supabase = new Client(url, key);
        await supabase.InitializeAsync();
        Debug.Log("Supabase connected!");
    }

    // ChapterManager should call this instead of reading studentChapterProgress
    public async Task<int> GetUnlockedChapter()
    {
        try
        {
            return await supabase.Rpc<int>("get_my_unlocked_chapter", new Dictionary<string, object>());
        }
        catch (Exception e)
        {
            Debug.LogError("Could not get unlocked chapter: " + e);
            return 1;
        }
    }

    // all 10 chapters, sorted 1 to 10. use chapterNumber as "the chapter number", NOT id
    public async Task<List<ChapterDbRow>> GetChapters()
    {
        try
        {
            var res = await supabase.From<ChapterDbRow>().Get();
            return res.Models.OrderBy(c => c.chapterNumber).ToList();
        }
        catch (Exception e)
        {
            Debug.LogError("Could not get chapters: " + e);
            return new List<ChapterDbRow>();
        }
    }

    // tutorial: use these in place of Student_Tutorial_Elementer
    public async Task<bool> HasFinishedTutorial()
    {
        try
        {
            var res = await supabase.From<StudentTutorialRow>().Get();
            return res.Models.Count > 0 && res.Models[0].hasFinishedTutorial;
        }
        catch (Exception e)
        {
            Debug.LogError("Could not read tutorial status: " + e);
            return false;
        }
    }

    public async Task MarkTutorialFinished()
    {
        try
        {
            await supabase.Rpc("mark_tutorial_finished", new Dictionary<string, object>());
        }
        catch (Exception e)
        {
            Debug.LogError("Could not mark tutorial finished: " + e);
        }
    }

    // ---------------- ANSWERS / SCORES / PROGRESS (all writes go through RPC, tables are read-only for students) ----------------
    // These never throw. On error they log and return -1 / false.

    // kind: "pre" for MCQ, "post" for PTQ.
    // each answer needs: questionLabel, question, answer, score
    // returns how many NEW rows were saved (0 = already answered before, first attempt only)
    public async Task<int> SubmitChapterAnswers(int chapterNumber, string kind, List<Dictionary<string, object>> answers)
    {
        try
        {
            return await supabase.Rpc<int>("submit_chapter_answers", new Dictionary<string, object>
            {
                { "chapterNumber", chapterNumber },
                { "kind", kind },
                { "answers", answers }
            });
        }
        catch (Exception e)
        {
            Debug.LogError("Could not save " + kind + " answers: " + e);
            return -1;
        }
    }

    // the server turns correct/total into a 0-100 percent. true = a new score was saved
    public async Task<bool> SubmitChapterScore(int chapterNumber, string kind, int correct, int total)
    {
        try
        {
            return await supabase.Rpc<bool>("submit_chapter_score", new Dictionary<string, object>
            {
                { "chapterNumber", chapterNumber },
                { "kind", kind },
                { "correct", correct },
                { "total", total }
            });
        }
        catch (Exception e)
        {
            Debug.LogError("Could not save " + kind + " score: " + e);
            return false;
        }
    }

    // completed = false when a chapter starts (updates last played), true when it ends (unlocks the next chapter)
    // returns the highest unlocked chapter, -1 on error
    public async Task<int> SaveChapterProgress(int chapterNumber, bool completed)
    {
        try
        {
            return await supabase.Rpc<int>("save_chapter_progress", new Dictionary<string, object>
            {
                { "chapterNumber", chapterNumber },
                { "completed", completed }
            });
        }
        catch (Exception e)
        {
            Debug.LogError("Could not save chapter progress: " + e);
            return -1;
        }
    }
// PASTE inside the SupabaseManager class (anywhere, e.g. above GetUnlockedChapter).
// Students log in with their student number. The website made their accounts as "<studentNumber>@app.local".

// returns null on success, or an error message to show on the login screen
    public async Task<string> StudentLogin(string studentNumber, string password)
    {
        try
        {
            if (supabase == null) return "Still connecting, try again in a second.";

            var session = await supabase.Auth.SignIn(studentNumber.Trim() + "@app.local", password);
            if (session == null || session.User == null) return "Login failed.";

            var res = await supabase.From<StudentRow>()
                .Filter("id", Postgrest.Constants.Operator.Equals, session.User.Id)
                .Get();

            if (res.Models.Count == 0)
            {
                await supabase.Auth.SignOut();
                return "This account is not a student account.";
            }

            StudentRow student = res.Models[0];
            if (student.status != "active")
            {
                await supabase.Auth.SignOut();
                return "This account is not active.";
            }

            SessionManager.Instance.SetStudent(student);
            return null;
        }
        catch (Exception e)
        {
            Debug.LogError("Login error: " + e);
            return "Wrong student number or password.";
        }
    }

    public async Task Logout()
    {
        try { await supabase.Auth.SignOut(); } catch (Exception e) { Debug.LogError(e); }
        SessionManager.Instance.Clear();
    }
}

// ---------------- NEW DATABASE MODELS ----------------

[Table("students")]
public class StudentRow : BaseModel
{
    [PrimaryKey("id")]
    public string id { get; set; }

    [Column("studentNumber")]
    public string studentNumber { get; set; }

    [Column("fullName")]
    public string fullName { get; set; }

    [Column("section")]
    public string section { get; set; }

    [Column("status")]
    public string status { get; set; }
}

[Table("chapters")]
public class ChapterDbRow : BaseModel
{
    [PrimaryKey("id")]
    public string id { get; set; }

    [Column("chapterNumber")]
    public int chapterNumber { get; set; }

    [Column("title")]
    public string title { get; set; }
}

[Table("student_tutorial")]
public class StudentTutorialRow : BaseModel
{
    [PrimaryKey("studentId")]
    public string studentId { get; set; }

    [Column("hasFinishedTutorial")]
    public bool hasFinishedTutorial { get; set; }
}

// ---------------- OLD MODELS ----------------
// Kept only so other scripts still compile. They do NOT work on the new database.
// Every one is [Obsolete], so Unity shows a yellow warning (CS0618) wherever it is still used.
// Fix those spots, then delete these classes.

[Obsolete("Old table. Use StudentRow (table 'students').")]
[Table("Students")]
public class Students : BaseModel
{
    [PrimaryKey("studentID")]
    public string studentID { get; set; }

    [Column("studentFirstName")]
    public string studentFirstName { get; set; }

    [Column("studentLastName")]
    public string studentLastName { get; set; }

    [Column("studentPassword")]
    public string studentPassword { get; set; }

    [Column("sectionID")]
    public int sectionID { get; set; }

    [Column("createdByTeacherID")]
    public int createdByTeacherID { get; set; }

    [Column("dateCreated")]
    public DateTime dateCreated { get; set; }

    [Column("studentChapterProgress")]
    public int studentChapterProgress { get; set; }

    [Column("lastPlayed")]
    public DateTime? lastPlayed { get; set; }
}

[Obsolete("Old table. Sections are not used by the game anymore.")]
[Table("Sections")]
public class Sections : BaseModel
{
    [PrimaryKey("sectionID")]
    public int sectionID { get; set; }

    [Column("sectionName")]
    public string sectionName { get; set; }

    [Column("teacherID")]
    public int teacherID { get; set; }

    [Column("sectionDateCreated")]
    public DateTime sectionDateCreated { get; set; }
}

[Obsolete("Old table. Use SupabaseManager.SubmitChapterAnswers().")]
[Table("Chapter_Answers_Record")]
public class Chapter_Answers_Record : BaseModel
{
    [PrimaryKey("id")]
    public int sectionID { get; set; }

    [Column("chapterID")]
    public int chapterID { get; set; }

    [Column("MCQ_PCQ")]
    public string MCQ_PCQ { get; set; }

    [Column("studentID")]
    public string studentID { get; set; }

    [Column("Question")]
    public string Question { get; set; }

    [Column("Answer")]
    public string Answer { get; set; }

    [Column("Score")]
    public int Score { get; set; }
}

[Obsolete("Old table. Use SupabaseManager.SubmitChapterScore().")]
[Table("Student_Chapter_Scores")]
public class Student_Chapter_Scores : BaseModel
{
    [PrimaryKey("id")]
    public int id { get; set; }

    [Column("studentID")]
    public string studentID { get; set; }

    [Column("chapterID")]
    public int chapterID { get; set; }

    [Column("MCQ_Score")]
    public int? MCQ_Score { get; set; }

    [Column("PTQ_Score")]
    public int? PTQ_Score { get; set; }
}

[Obsolete("Old table. Use HasFinishedTutorial() / MarkTutorialFinished().")]
[Table("Student_Tutorial_Elementer")]
public class Student_Tutorial_Elementer : BaseModel
{
    [PrimaryKey("id")]
    public int id { get; set; }

    [Column("studentID")]
    public string studentID { get; set; }

    [Column("hasFinishedTutorial")]
    public bool hasFinishedTutorial { get; set; }
}