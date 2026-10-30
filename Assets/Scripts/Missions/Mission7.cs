using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Mission7 : MissionBase
{
    public override int ChapterNumber => 7;

    public override string RequiredMixResult => "Sulfur Dioxide";
    public override string MixInstructions => "Make the gas that will clear the weevils.\nNot sure how? Ask Clarence AI for a hint.";

    private const string chapterTitle = "The Grain Fumigation";

    [Header("Chapter 7 only")]
    public GameObject solPrefab;          // Trader Sol
    public Sprite desertBackground;      // desert caravan camp (empty = forest night)

    private List<Action> steps;

    private Sprite Bg { get { return desertBackground != null ? desertBackground : forestNightBackground; } }

    private void StudentSays(params string[] lines) { Say(studentFirstName, emptyPrefab, Bg, lines); }
    private void SolSays(params string[] lines) { Say("Trader Sol", solPrefab, Bg, lines); }
    private void AiSays(params string[] lines) { Say("Clarence AI", elementerAIPrefab, Bg, lines); }

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
            Debug.Log("End of mission 7");
            FinishChapter();
        }
    }

    private void BuildSteps()
    {
        steps = new List<Action>
        {
            // ---------------- OPENING VIDEO (clip on VideoPlayer, slot 1) ----------------
            () =>
            {
                dialogueObject.SetActive(false);
                backButton.interactable = false;
                nextButton.interactable = false;

                videoPlayerObject.SetActive(true);
                videoPlayer.Play();
                videoPlayer.loopPointReached += OnVideoFinished;
            },

            // ---------------- ARRIVAL ----------------
            () => { StartCoroutine(WaitScene()); StudentSays("It's boiling out here."); },
            () => AiSays("Stay alert.", "Someone here needs help."),
            () => SolSays("Don't touch those sacks! Look inside."),
            () => StudentSays("Those are weevils. They're eating all the grain."),
            () => SolSays("Yes. My old note says to fumigate the storeroom with sulfur dioxide.",
                          "Exactly 2 moles. Any less and the weevils survive.",
                          "But my scale only shows grams. How do I know when I have 2 moles?"),
            () => StudentSays("Sulfur dioxide is SO2. I can make it from sulfur and oxygen.",
                              "But first I need to turn grams into moles.", "*You open the Elementer.*"),

            // ---------------- THE STORY TEACHES: grams to moles ----------------
            () => AiSays("Here is the equation: S + O2 -> SO2.",
                         "1 sulfur and 2 oxygen on each side. It's balanced.",
                         "Now, grams to moles. Think of eggs."),
            () => AiSays("One dozen eggs weighs 600 grams. You have 1200 grams of eggs.",
                         "How many dozens is that?"),
            () => StudentSays("1200 divided by 600. That's 2 dozen."),
            () => AiSays("You just did it. Moles work the same way.",
                         "Moles = mass / molar mass.",
                         "Mass is in grams. Molar mass is in g/mol."),
            () => StudentSays("So I divide the grams by the molar mass. Like the eggs."),
            () => AiSays("Example: 24 grams of carbon. Carbon is 12 g/mol.",
                         "24 / 12 = 2 moles."),
            () => StudentSays("Easy. What about a compound?"),
            () => AiSays("Find the molar mass first by adding every atom. Then divide.",
                         "Example: 80 grams of MgO. Magnesium is 24 and oxygen is 16, so MgO is 40 g/mol.",
                         "80 / 40 = 2 moles."),
            () => StudentSays("Add the atoms first, then divide the grams by that."),
            () => SolSays("Sounds simple enough. Show me.", "How do you turn grams into moles?"),

            // ---------------- MCQs ----------------
            () => ShowMCQ("Chapter "+MissionBase.Instance.ChapterNumber +"| MCQ 1",
                "How do you change grams into moles?",
                new string[] { "Divide the mass by the molar mass", "Multiply the mass by the molar mass", "Add the mass and the molar mass" },
                "Divide the mass by the molar mass",
                new string[] { "Correct!", "Moles = mass / molar mass." },
                new string[] { "Nice try! But unfortunately your answer is wrong!", "Moles = mass / molar mass." },
                "Clarence AI", elementerAIPrefab, Bg),
            () => StudentSays("Divide the mass by the molar mass."),
            () => SolSays("Try it. I have a flask of water that weighs 36 grams."),

            () => ShowMCQ("Chapter "+MissionBase.Instance.ChapterNumber +"| MCQ 2",
                "How many moles are in 36 g of H2O? (molar mass 18 g/mol)",
                new string[] { "648 mol", "18 mol", "2 mol" },
                "2 mol",
                new string[] { "Correct!", "36 / 18 = 2 moles." },
                new string[] { "Nice try! But unfortunately your answer is wrong!", "36 / 18 = 2 moles." },
                "Clarence AI", elementerAIPrefab, Bg),
            () => StudentSays("36 / 18 = 2 moles."),
            () => SolSays("Now a harder one. I have a jar of carbon dioxide that weighs 88 grams."),

            () => ShowMCQ("Chapter "+MissionBase.Instance.ChapterNumber +"| MCQ 3",
                "How many moles are in 88 g of CO2? (C = 12, O = 16)",
                new string[] { "3872 mol", "2 mol", "44 mol" },
                "2 mol",
                new string[] { "Correct!", "CO2 is 12 + 16 + 16 = 44 g/mol. Then 88 / 44 = 2 moles." },
                new string[] { "Nice try! But unfortunately your answer is wrong!", "CO2 is 12 + 16 + 16 = 44 g/mol. Then 88 / 44 = 2 moles." },
                "Clarence AI", elementerAIPrefab, Bg),
            () => StudentSays("CO2 is 12 + 16 + 16 = 44 g/mol. Then 88 / 44 = 2 moles.", "Got it. Now I can check the sulfur dioxide."),

            // ---------------- ELEMENTER MIX ----------------
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
                StudentSays("It worked! Pale gas is filling the canister!");
            },

            () => ShowTextCard("Trader Sol weighs the gas on his scale. It reads 128 grams."),

            () => StudentSays("SO2 is 32 + 16 + 16 = 64 g/mol.", "128 / 64 = 2 moles. That's exactly what your note says."),
            () => SolSays("Perfect!"),

            () => ShowTextCard("Sol seals the storeroom and releases the gas. Hours later, he opens the door. The weevils are gone..."),

            () => SolSays("Keep out until the room airs. The gas is dangerous.",
                          "The grain is saved. Thank you, traveler.",
                          "Take this gold coin. It's all I can spare."),

            // ---------------- REPAIR MATERIAL: GOLD ----------------
            () => AiSays("*The Elementer beeps.*", "REPAIR MATERIAL REQUIRED: GOLD."),
            () => StudentSays("That's the coin you just gave me.", "*You place the gold into the Elementer.*"),
            () => AiSays("GOLD DETECTED.", "DATABASE RECALIBRATION STARTED.", "ASSISTANCE REQUIRED."),

            // ---------------- PTQs ----------------
            () => ShowPTQ("Chapter "+MissionBase.Instance.ChapterNumber +"| PTQ 1",
                "A sample is 36 g of carbon. Carbon's molar mass is 12 g/mol. Which calculation gives the moles?",
                new string[] { "36 / 12", "36 x 12", "36 - 12" },
                "36 / 12",
                "Divide the mass by the molar mass: 36 / 12 = 3 moles!", Bg),
            () => ShowPTQ("Chapter "+MissionBase.Instance.ChapterNumber +"| PTQ 2",
                "How many moles are in 112 g of iron? (Fe = 56 g/mol)",
                new string[] { "56 mol", "2 mol", "6272 mol" },
                "2 mol",
                "112 / 56 = 2 moles!", Bg),
            () => ShowPTQ("Chapter "+MissionBase.Instance.ChapterNumber +"| PTQ 3",
                "How many moles are in 160 g of MgO? (Mg = 24, O = 16)",
                new string[] { "6400 mol", "120 mol", "4 mol" },
                "4 mol",
                "MgO is 24 + 16 = 40 g/mol. Then 160 / 40 = 4 moles!", Bg),

            // ---------------- RECALIBRATION DONE + ELEMENTS UNLOCKED ----------------
            () => AiSays("DATABASE RECALIBRATION COMPLETE.", "MOLE CALCULATION FILES RESTORED.", ElementLocks.Unlock(7)),

            // ---------------- CHAPTER END ----------------
            () => SolSays("May your road be shorter than your water."),
            () => AiSays(studentFirstName + ", the gold boosted my tracker a lot.",
                         "Signal strength: 70%. We're close."),
            () => StudentSays("Is he moving at all?"),
            () => AiSays("No. I've checked every reading since we started. His location hasn't changed once."),
            () => StudentSays("So he's staying in one spot?"),
            () => AiSays("Or he can't leave. Either he's stuck somewhere, or someone is holding him there."),
            () => StudentSays("Then we need to hurry."),
            () => AiSays("NEXT TRACE DETECTED. PORTAL CHARGING."),
            () => StudentSays("Hang in there, Professor.", "*The Elementer glows, ready for the next jump.*"),

            // ---------------- ENDING VIDEO (clip on VideoPlayer2, slot 2) ----------------
            () =>
            {
                dialogueObject.SetActive(false);
                backButton.interactable = false;
                nextButton.interactable = false;

                videoPlayerObject2.SetActive(true);
                videoPlayer2.Play();
                videoPlayer2.loopPointReached += OnVideoFinished;
            },

            () => ShowTextCard("Chapter 7\n" + chapterTitle + "\nComplete",
                               "\n\nPress the screen to go back to the main menu..."),
        };
    }
}
