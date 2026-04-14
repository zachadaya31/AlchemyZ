using TMPro;
using UnityEngine;

public class AccountSetter : MonoBehaviour
{
    public TextMeshProUGUI tmpStudentID;
    public TextMeshProUGUI tmpStudentSection;
    public TextMeshProUGUI tmpStudentName;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tmpStudentID.SetText(UserSession.Instance.currentUser.studentID);
        tmpStudentSection.SetText(UserSession.Instance.currentUser.studentSection);
        tmpStudentName.SetText(UserSession.Instance.currentUser.studentName);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
