using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Mission3 : MissionBase
{
    public override int ChapterNumber => 3;

    public override string RequiredMixResult => "Magnesium Oxide";
    public override string MixInstructions => "Create Magnesium Oxide.\nNot sure how? Ask Clarence AI for a hint.";

    private const string chapterTitle = "The Healer's Remedy";

    [Header("Chapter 3 only")]
    public GameObject liraPrefab;     // Healer Lira
    public Sprite campBackground;     // forest camp at dusk (empty = forest night)

    private List<Action> steps;

    private Sprite Camp { get { return campBackground != null ? campBackground : forestNightBackground; } }

    private void StudentSays(params string[] lines) { Say(studentFirstName, emptyPrefab, Camp, lines); }
    private void LiraSays(params string[] lines) { Say("Healer Lira", liraPrefab, Camp, lines); }
    private void AiSays(params string[] lines) { Say("Clarence AI", elementerAIPrefab, Camp, lines); }

    protected override void Awake()
    {
        base.Awake();
        BuildSteps();
    }

    public override void nextScene()
    {
        currentScene++;

        if (currentScene <= steps.Count)
        {
            steps[currentScene - 1]();
        }
        else
        {
            Debug.Log("End of mission 3");
            FinishChapter();
        }
    }

    private void BuildSteps()
    {
        steps = new List<Action>
        {
            () =>
            {
                dialogueObject.SetActive(false);
                backButton.interactable = false;
                nextButton.interactable = false;

                videoPlayerObject.SetActive(true);
                videoPlayer.Play();
                videoPlayer.loopPointReached += OnVideoFinished;
            },
            () => { StartCoroutine(WaitScene()); StudentSays("This place feels different from the last camp."); },
            () => LiraSays("You there! Are you a healer?"),
            () => StudentSays("Not exactly. I have a machine, though."),
            () => LiraSays("Then you are just in time.", "Half my soldiers are sick from spoiled food.", "I need a remedy called magnesium oxide."),
            () => StudentSays("Magnesium oxide? Let me check the Elementer."),
            () => AiSays("SCANNING NAME: MAGNESIUM OXIDE.", "NAMING RULE 1: THE METAL IS NAMED FIRST."),
            () => StudentSays("So the metal comes first, like Sodium in Sodium Chloride."),
            () => AiSays("NAMING RULE 2: THE NONMETAL IS NAMED SECOND AND ITS ENDING CHANGES TO \"-IDE\"."),
            () => StudentSays("Chlorine became Chloride. Oxygen becomes Oxide."),
            () => LiraSays("Strange words, but I follow."),
            () => AiSays("NAMING RULE 3: ATOMS TRADE ELECTRONS.",
                         "MAGNESIUM GIVES AWAY 2 ELECTRONS. OXYGEN TAKES 2 ELECTRONS.",
                         "THE FORMULA MUST BE BALANCED, WITH NOTHING LEFT OVER."),
            () => StudentSays("So Magnesium gives and Oxygen takes. It's like a trade."),
            () => LiraSays("Now you are making sense. Tell me, which metal is in this remedy?"),

            // ---------------- MCQs ----------------
            () => ShowMCQ("Chapter "+MissionBase.Instance.ChapterNumber +"| MCQ 1",
                "Which element is the metal in magnesium oxide?",
                new string[] { "Oxygen", "Magnesium", "Oxide" },
                "Magnesium",
                new string[] { "Correct!", "Magnesium is the metal in magnesium oxide." },
                new string[] { "Nice try! But unfortunately your answer is wrong!", "Magnesium is the metal in magnesium oxide." },
                "Clarence AI", elementerAIPrefab, Camp),
            () => StudentSays("Magnesium. The metal comes first in the name."),
            () => LiraSays("Good. And the second part of the name?"),

            () => ShowMCQ("Chapter "+MissionBase.Instance.ChapterNumber +"| MCQ 2",
                "What does the nonmetal oxygen become in the name of a compound?",
                new string[] { "Oxygenide", "Oxide", "Oxygen" },
                "Oxide",
                new string[] { "Correct!", "Oxygen becomes Oxide. The ending changes to \"-ide\"." },
                new string[] { "Nice try! But unfortunately your answer is wrong!", "Oxygen becomes Oxide. The ending changes to \"-ide\"." },
                "Clarence AI", elementerAIPrefab, Camp),
            () => StudentSays("Oxide. The ending changes to \"-ide\"."),
            () => LiraSays("Then how many of each do you need?"),

            () => ShowMCQ("Chapter "+MissionBase.Instance.ChapterNumber +"| MCQ 3",
                "Magnesium gives away 2 electrons and oxygen takes 2 electrons. How many of each does magnesium oxide need?",
                new string[] { "1 Magnesium and 1 Oxygen", "2 Magnesium and 1 Oxygen", "1 Magnesium and 2 Oxygen" },
                "1 Magnesium and 1 Oxygen",
                new string[] { "Correct!", "Magnesium gives 2 and oxygen takes 2, so one of each balances the trade." },
                new string[] { "Nice try! But unfortunately your answer is wrong!", "Magnesium gives 2 and oxygen takes 2, so one of each balances the trade." },
                "Clarence AI", elementerAIPrefab, Camp),
            () => StudentSays("One gives 2 and one takes 2. One of each is enough."),

            // ---------------- ELEMENTER MIX (Magnesium Oxide) ----------------
            () =>
            {
                dialogueObject.SetActive(false);
                SceneManager.LoadScene("Elementer", LoadSceneMode.Additive);
                missionCamera.SetActive(false);
                missionEventSystem.SetActive(false);
            },
            () =>
            {
                missionCamera.SetActive(true);
                missionEventSystem.SetActive(true);
                _ = SceneManager.UnloadSceneAsync("Elementer");
                StudentSays("It worked!");
            },
            () => LiraSays("Wonderful! Magnesium oxide, just as the old books say."),

            () => ShowTextCard("Healer Lira gives the powder to the sick soldiers..."),

            () => LiraSays("They already look better. You have my thanks.", "Please, take this copper bowl. I have no use for it."),

            // ---------------- REPAIR MATERIAL: COPPER ----------------
            () => AiSays("*The Elementer beeps.*", "REPAIR MATERIAL REQUIRED: COPPER."),
            () => StudentSays("That's exactly what you just gave me.", "*You place the copper into the Elementer.*"),
            () => AiSays("COPPER DETECTED.", "DATABASE RECALIBRATION INITIATED.", "ASSISTANCE REQUIRED."),

            // ---------------- PTQs ----------------
            () => ShowPTQ("Chapter "+MissionBase.Instance.ChapterNumber +"| PTQ 1",
                "A compound has lithium (metal) and fluorine (nonmetal). Which element is named first in its name?",
                new string[] { "Fluorine", "Lithium", "It does not matter" },
                "Lithium",
                "The metal is named first, so Lithium comes first!", Camp),
            () => ShowPTQ("Chapter "+MissionBase.Instance.ChapterNumber +"| PTQ 2",
                "Sodium and sulfur form a compound. Which is the correct name?",
                new string[] { "Sodium sulfur", "Sodium sulfide", "Sulfide sodium" },
                "Sodium sulfide",
                "The metal goes first and the nonmetal ends in -ide: Sodium sulfide!", Camp),
            () => ShowPTQ("Chapter "+MissionBase.Instance.ChapterNumber +"| PTQ 3",
                "Calcium gives away 2 electrons and oxygen takes 2 electrons. What is the formula of calcium oxide?",
                new string[] { "Ca2O", "CaO2", "CaO" },
                "CaO",
                "Calcium gives 2 and oxygen takes 2, so you need one of each: CaO!", Camp),

            // ---------------- RECALIBRATION DONE + ELEMENTS UNLOCKED ----------------
            () => AiSays("DATABASE RECALIBRATION COMPLETE.", "NAMING AND FORMULA FILES RESTORED.", ElementLocks.Unlock(3)),

            () => AiSays(studentFirstName + ", I found something.",
                         "A faint trace of the professor's signal.",
                         "It is coming from a different time period.",
                         "Let's follow it.",
                         "*The Elementer opens a small glowing gateway.*"),

            () =>
            {
                dialogueObject.SetActive(false);
                backButton.interactable = false;
                nextButton.interactable = false;

                videoPlayerObject2.SetActive(true);
                videoPlayer2.Play();
                videoPlayer2.loopPointReached += OnVideoFinished;
            },

            // ---------------- END OF CHAPTER ----------------
            () => ShowTextCard("Chapter 3\n" + chapterTitle + "\nComplete",
                               "\n\nPress the screen to go back to the main menu..."),
        };
    }
}