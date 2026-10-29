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

    [Header("Optional: auto-found if left empty")]
    public ElementSpawner spawner;       // ButtonPressManager has this

    int step = 0;
    int baseline = 0;
    bool clearPressed, zachPressed, mixPressed, tapped, chatSeen;

    // counts from the frame BEFORE Mix was pressed (Mix removes the elements)
    int lastH, lastO, lastTotal;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (spawner == null) spawner = FindAnyObjectByType<ElementSpawner>();

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

    // counts spawned elements by name, e.g. "Hydrogen"
    int CountElement(string elementName)
    {
        int count = 0;
        foreach (GameObject go in spawner.GetSpawnedElements())
        {
            if (go == null) continue;
            string cleanName = go.name.Replace("(Clone)", "").Trim();
            if (cleanName.ToLower() == elementName.ToLower()) count++;
        }
        return count;
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
            case 6: instructionText.text = "Need help? Press the button in the top right to chat with Clarence AI."; break;
            case 7: instructionText.text = "Ask Clarence anything, then close the chat when you're done."; break;
            case 8: instructionText.text = "Now let's create Water! Place 2 Hydrogen and 1 Oxygen, then press Mix."; break;
            case 9:
                instructionText.text = "Uh oh... something is wrong with the Elementer!";
                Invoke("Finish", 0.5f);
                break;
        }
    }

    void CheckWater()
    {
        // use the snapshot, because MixManager already removed the elements by now
        int h = lastH;
        int o = lastO;
        int total = lastTotal;
        Debug.Log("Tutorial check: Hydrogen=" + h + " Oxygen=" + o + " Total=" + total);

        // correct: exactly 2 Hydrogen, 1 Oxygen and nothing else
        if (h == 2 && o == 1 && total == 3)
        {
            ShowStep(9);
            return;
        }

        string msg = "";
        if (h < 2) msg += "You're missing " + (2 - h) + " more Hydrogen! ";
        if (o < 1) msg += "You're missing " + (1 - o) + " more Oxygen! ";
        if (h > 2 || o > 1) msg += "Too many! Remember, water only needs 2 Hydrogen atoms and 1 Oxygen atom. ";
        if (total > h + o) msg += "Remove the other elements too. ";

        instructionText.text = msg;
    }

    void TakeSnapshot()
    {
        lastH = CountElement("Hydrogen");
        lastO = CountElement("Oxygen");
        lastTotal = spawner.GetSpawnedElements().Count;
    }

    // goes to the next Mission1 scene and removes the tutorial scene
    void Finish()
    {
        if (Mission1.Instance != null)
        {
            MissionBase.Instance.nextScene();
            SceneManager.UnloadSceneAsync(gameObject.scene);
        }
        else
        {
            Debug.Log("Tutorial done. No Mission1 found, so staying in this scene.");

        }

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
            case 8:
                if (!mixPressed)
                {
                    TakeSnapshot();
                }
                else
                {
                    mixPressed = false;
                    CheckWater();
                }
                break;
        }
    }

}