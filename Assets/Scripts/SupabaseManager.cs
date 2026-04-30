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
    // SupabaseManager.Instance.supabase
    // YAN EENETER SA LAHAT NG POSTRESQL STATEMENTS

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
    public int studentID{get;set;}

    [Column("studentFirstName")]
    public string studentFirstName{get;set;}

    [Column("studentLastName")]
    public string studentLastName{get;set;}

    [Column("studentPassword")]
    public string studentPassword{get;set;}

    [Column("sectionID")]
    public int sectionID{get;set;}

    [Column("createdByTeacherID")]
    public int createdByTeacherID{get;set;}

    [Column("dateCreated")]
    public DateTime dateCreated{get;set;}
}

[Table("Sections")]
public class Sections : BaseModel
{
    [PrimaryKey("sectionID")]
    public int sectionID{get;set;}

    [Column("sectionName")]
    public string sectionName{get;set;}

    [Column("teacherID")]
    public int teacherID{get;set;}

    [Column("sectionDateCreated")]
    public DateTime sectionDateCreated{get;set;}
}

