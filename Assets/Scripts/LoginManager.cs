using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Supabase.Gotrue;
using TMPro;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    public TMP_InputField inputID;
    public TMP_InputField inputPassword;
    public TextMeshProUGUI errorText;

    public void loginButtonClicked()
    {
        int tempID = int.Parse(inputID.text.Trim());
        string tempPassword = inputPassword.text.Trim();

        login(tempID, tempPassword);
    }

    public async void login(int tempID, string tempPassword)
    {

        int studentID = tempID; 
        string studentPassword = tempPassword;

        var responseStudent = await SupabaseManager.Instance.supabase
        .From<Students>()
        .Where(row => row.studentID == studentID)
        .Single();

        var responseSection = await SupabaseManager.Instance.supabase
        .From<Sections>()
        .Where(row => row.sectionID == responseStudent.sectionID)
        .Single();
        
        if (responseStudent == null)
        {
            errorText.gameObject.SetActive(true);
            errorText.SetText("No Account with this Student ID is found.");
        } 
        else if (responseStudent.studentPassword != studentPassword)
        {
            errorText.gameObject.SetActive(true);
            errorText.SetText("Wrong password entered.");
        }
        else
        {
            SessionManager.Instance.studentID = responseStudent.studentID;
            SessionManager.Instance.studentFirstName = responseStudent.studentFirstName;
            SessionManager.Instance.studentLastName = responseStudent.studentLastName;
            SessionManager.Instance.sectionName = responseSection.sectionName;
            SceneManager.LoadScene("MainMenu");
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
