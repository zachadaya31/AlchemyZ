using UnityEngine;
using Supabase;
using Postgrest.Models;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine.UIElements;
using TMPro;
using System;
using Postgrest.Attributes;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


// TABLE NG SQL TO
[Table("StudentAccount")]
public class StudentAccount : BaseModel
{
    [PrimaryKey("studentID")]
    public string studentID {get; set;}
    public string studentName {get; set;}
    public string studentSection {get; set;}
    public string studentPassword {get; set;}
    
}

public class SupabaseManager : MonoBehaviour
{
    private string url = "https://uhyxunjlxbgcagasgzde.supabase.co";
    private string key ="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InVoeXh1bmpseGJnY2FnYXNnemRlIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NzU4NDE5MjMsImV4cCI6MjA5MTQxNzkyM30.PyC8KZ1RQ4BVKTHJDy2HWQlmoXifQgbaSwxqB8m86yQ";
    private Client supabase;
    public TMP_InputField userInputID;
    public TMP_InputField userInputPassword;
    public TextMeshProUGUI textWrongPassword;

    void Awake()
    {
        supabase = new Client(url, key);
    }

    public void loginButton()
    {
        attemptLogin(userInputID.text.Trim(), userInputPassword.text.Trim());
    }

    public async void attemptLogin(string inputID, string inputPassword)
    {
        var response = await supabase.From<StudentAccount>()
        .Where(x => x.studentID == inputID)
        .Get();

        var student = response.Models.FirstOrDefault();

        if (student == null)
        {
            Debug.Log("No student is found");
        } 
        else if (student.studentPassword == inputPassword)
        {
            UserSession.Instance.currentUser = student;
            SceneManager.LoadScene("MainMenu");
        }
        else
        {
            textWrongPassword.gameObject.SetActive(true);
            Debug.Log("Wrong password");
        }
        

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
