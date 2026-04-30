using TMPro;
using UnityEngine;

public class AccountSetter : MonoBehaviour
{
    public TextMeshProUGUI placeholderStudentID;
    public TextMeshProUGUI placeholderStudentSection;
    public TextMeshProUGUI placeholderStudentName;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        placeholderStudentID.SetText(SessionManager.Instance.studentID.ToString());
        placeholderStudentSection.SetText(SessionManager.Instance.sectionName);
        placeholderStudentName.SetText(SessionManager.Instance.studentFirstName+" "+SessionManager.Instance.studentLastName);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
