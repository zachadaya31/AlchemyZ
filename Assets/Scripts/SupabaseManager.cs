using UnityEngine;
using Supabase;
using System;
using Postgrest.Attributes;
using Postgrest.Models;

public class SupabaseManager : MonoBehaviour
{
    private string url = "https://uhyxunjlxbgcagasgzde.supabase.co";
    private string key = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InVoeXh1bmpseGJnY2FnYXNnemRlIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NzU4NDE5MjMsImV4cCI6MjA5MTQxNzkyM30.PyC8KZ1RQ4BVKTHJDy2HWQlmoXifQgbaSwxqB8m86yQ";

    public Client supabase;
    public static SupabaseManager Instance;

    async void Awake()
    {
        Instance = this;
        supabase = new Client(url, key);
        await supabase.InitializeAsync();
        Debug.Log("Supabase connected!");
    }
}

[Table("Students")]
public class Students : BaseModel
{
    [PrimaryKey("studentID")]
    public string studentID { get; set; } // Changed from int to string

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
}

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

[Table("Chapter_Answers_Record")]
public class Chapter_Answers_Record : BaseModel
{
    [PrimaryKey("id")]
    public int sectionID { get; set; } // Note: Check if your primary key column in this table is actually named "id" instead of sectionID

    [Column("chapterID")]
    public int chapterID { get; set; }

    [Column("MCQ_PCQ")]
    public string MCQ_PCQ { get; set; }

    [Column("studentID")]
    public string studentID { get; set; } // Changed from int to string

    [Column("Question")]
    public string Question { get; set; }

    [Column("Answer")]
    public string Answer { get; set; }

    [Column("Score")]
    public int Score { get; set; }
}

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