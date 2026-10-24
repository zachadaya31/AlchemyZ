using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager instance;

    [Header("Drag these in")]
    public TMP_Text instructionText;     // the text inside the Instructions object
    public GameObject elementLibrary;    // Element Library
    public GameObject chatbox;           // UI - Chatbox
    public Button clearButton;           // Button - Clear
    public Button profZachButton;        // Button - Prof Zach
    public Button mixButton;             // Button - Mix

    [Header("Optional: scene to load after Mix")]
    public string nextSceneName = "";

    int step = 0;
    int baseline = 0;
    bool clearPressed, zachPressed, mixPressed, tapped, chatSeen;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        clearButton.onClick.AddListener(() => clearPressed = true);
        profZachButton.onClick.AddListener(() => zachPressed = true);
        mixButton.onClick.AddListener(() => mixPressed = true);
        ShowStep(0);    
    }

    // ElementNametagTap calls this when an element is tapped
    public static void Tapped()
    {
        if (instance != null) instance.tapped = true;
    }

    int ElementCount()
    {
        return FindObjectsByType<ElementNametagTap>().Length;
    }

    void ShowStep(int n)
    {
        step = n;
        baseline = ElementCount();
        clearPressed = false;
        zachPressed = false;
        mixPressed = false;
        tapped = false;
        chatSeen = false;

        switch (step)
        {
            case 0: instructionText.text = "Welcome to the Elementer! Press the Element button to open the Element Library."; break;
            case 1: instructionText.text = "Nice! Now pick Iron from the library."; break;
            case 2: instructionText.text = "Great! Now tap the floor to place the Iron."; break;
            case 3: instructionText.text = "Tap the Iron to see extra information about it."; break;
            case 4: instructionText.text = "Now press and hold the Iron to delete it."; break;
            case 5: instructionText.text = "Place a few more elements, then press Clear to remove everything at once."; break;
            case 6: instructionText.text = "Need help? Press Prof Zach to chat with the AI."; break;
            case 7: instructionText.text = "Ask Prof Zach anything, then close the chat when you're done."; break;
            case 8: instructionText.text = "Last step! Press Mix to see what happens."; break;
            case 9:
                instructionText.text = "Uh oh... something is wrong with the Elementer!";
                if (nextSceneName != "") Invoke("LoadNext", 3f);
                break;
        }
    }

    void LoadNext()
    {
        SceneManager.LoadScene(nextSceneName);
    }

    void Update()
    {
        switch (step)
        {
            case 0: if (elementLibrary.activeInHierarchy) ShowStep(1); break;
            case 1: if (ElementSelecter.elementSelected == "Iron") ShowStep(2); break;
            case 2: if (ElementCount() > baseline) ShowStep(3); break;
            case 3: if (tapped) ShowStep(4); break;
            case 4: if (ElementCount() < baseline) ShowStep(5); break;
            case 5: if (clearPressed) ShowStep(6); break;
            case 6: if (zachPressed) ShowStep(7); break;
            case 7:
                if (chatbox.activeInHierarchy) chatSeen = true;
                if (chatSeen && !chatbox.activeInHierarchy) ShowStep(8);
                break;
            case 8: if (mixPressed) ShowStep(9); break;
        }
    }
}