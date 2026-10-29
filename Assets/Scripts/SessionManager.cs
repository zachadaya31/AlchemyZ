using UnityEngine;

// Holds who is logged in. Filled by SupabaseManager.StudentLogin() after a successful sign-in.
public class SessionManager : MonoBehaviour
{
    
    public static SessionManager Instance;

    // NEW database
    public string studentID;        // auth user id (uuid as text)
    public string studentNumber;    // what they type to log in, ex: 26005
    public string fullName;
    public string section;
    public string studentFirstName; // MissionBase reads this

    // OLD fields, kept only so old scripts still compile. Delete once nothing uses them.
    public string studentLastName;
    public int sectionID;
    public string sectionName;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }



    public void SetStudent(StudentRow s)
    {
        studentID = s.id;
        studentNumber = s.studentNumber;
        fullName = s.fullName;
        section = s.section;
        sectionName = s.section;

        string[] parts = string.IsNullOrWhiteSpace(s.fullName)
            ? new string[0]
            : s.fullName.Trim().Split(' ');
        studentFirstName = parts.Length > 0 ? parts[0] : "Student";
        studentLastName = parts.Length > 1 ? parts[parts.Length - 1] : "";
    }

    public void Clear()
    {
        studentID = studentNumber = fullName = section = studentFirstName = studentLastName = sectionName = "";
        sectionID = 0;
    }


}