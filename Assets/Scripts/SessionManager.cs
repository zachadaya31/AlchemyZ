using Supabase.Gotrue;
using UnityEngine;

public class SessionManager : MonoBehaviour
{
    public static SessionManager Instance;

    public int studentID;
    public string studentFirstName;
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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
