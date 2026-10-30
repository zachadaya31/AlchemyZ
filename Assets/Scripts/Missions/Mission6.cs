using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Mission6 : MissionBase
{
    public override int ChapterNumber => 6;

    public override string RequiredMixResult => "Silicon Dioxide";
    public override string MixInstructions => "Create Silicon Dioxide";

    private const string chapterTitle = "The Lighthouse Lens";

    [Header("Chapter 6 only")]
    public GameObject danPrefab;          // Keeper Dan
    public Sprite coastBackground;      // stormy coast and lighthouse at dusk (empty = forest night)

    private List<Action> steps;

    private Sprite Bg { get { return coastBackground != null ? coastBackground : forestNightBackground; } }

    private void StudentSays(params string[] lines) { Say(studentFirstName, emptyPrefab, Bg, lines); }
    private void DanSays(params string[] lines) { Say("Keeper Dan", danPrefab, Bg, lines); }
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
            Debug.Log("End of mission 6");
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
            () => { StartCoroutine(WaitScene()); StudentSays("That lighthouse is barely shining."); },
            () => AiSays("Someone up there might need help."),
            () => DanSays("You there! Come up here, quick!"),
            () => DanSays("A storm cracked the lens. The light is too weak and ships can't see it.",
                          "The old keeper's note says a new lens needs glass made from silica. Exactly 60 grams.",
                          "Too much and it won't fit the frame. Too little and it will crack again."),
            () => StudentSays("Silica is silicon dioxide, SiO2. I can make it from silicon and oxygen.", "*You open the Elementer.*"),
            () => AiSays("Here is the equation: Si + O2 -> SiO2.",
                         "Count it. 1 silicon and 2 oxygen on the left. 1 silicon and 2 oxygen on the right.",
                         "It's already balanced."),
            () => StudentSays("But how much is 60 grams?", "I can't just guess it."),

            // ---------------- THE STORY TEACHES: mole and molar mass ----------------
            () => AiSays("Atoms are far too tiny to count one by one, so chemists count them in groups.",
                         "You know a dozen means 12 of anything. 12 eggs, 12 pencils, 12 shoes.",
                         "A mole is the same idea, just a much bigger group: 6.02 x 10^23 particles."),
            () => StudentSays("So a mole is just a really, really big dozen."),
            () => AiSays("Exactly. Now here's the useful part.",
                         "A dozen eggs and a dozen grapes both have 12 pieces, but they don't weigh the same.",
                         "Same with atoms. One mole of each element has its own weight."),
            () => StudentSays("And that weight is the molar mass?"),
            () => AiSays("Yes. Molar mass is the weight of one mole, in grams. We write it as g/mol.",
                         "For an element, it's the number from the periodic table. Silicon is 28 g/mol. Oxygen is 16 g/mol."),
            () => StudentSays("So one mole of silicon weighs 28 grams.", "What about a compound like SiO2?"),
            () => AiSays("For a compound, add up every atom in the formula. The small numbers tell you how many of each.",
                         "Try it on water. H2O has 2 hydrogen and 1 oxygen. Hydrogen is 1, oxygen is 16."),
            () => StudentSays("2 hydrogen is 1 + 1 = 2. Plus 16 for oxygen.", "That's 18 g/mol."),
            () => AiSays("Correct. Add every atom, never multiply."),
            () => DanSays("I understood none of that, but your numbers sound careful.", "Tell me what you've learned."),

            // ---------------- MCQs ----------------
            () => ShowMCQ("Chapter "+MissionBase.Instance.ChapterNumber +"| MCQ 1",
                "What is molar mass?",
                new string[] { "The mass of one mole of a substance, in grams", "The number of atoms in a molecule", "How fast a substance reacts" },
                "The mass of one mole of a substance, in grams",
                new string[] { "Correct!", "Molar mass is how many grams one mole weighs." },
                new string[] { "Nice try! But unfortunately your answer is wrong!", "Molar mass is how many grams one mole weighs." },
                "Clarence AI", elementerAIPrefab, Bg),
            () => StudentSays("It's how many grams one mole weighs."),
            () => DanSays("And how do you find it for a compound?"),

            () => ShowMCQ("Chapter "+MissionBase.Instance.ChapterNumber +"| MCQ 2",
                "How do you find the molar mass of a compound?",
                new string[] { "Add the atomic masses of all the atoms in its formula", "Multiply the atomic masses of its elements", "Use only the heaviest atom" },
                "Add the atomic masses of all the atoms in its formula",
                new string[] { "Correct!", "Add the masses of every atom in the formula." },
                new string[] { "Nice try! But unfortunately your answer is wrong!", "Add the masses of every atom in the formula." },
                "Clarence AI", elementerAIPrefab, Bg),
            () => StudentSays("Add up every atom in the formula."),
            () => DanSays("Then work out silica for me."),

            () => ShowMCQ("Chapter "+MissionBase.Instance.ChapterNumber +"| MCQ 3",
                "What is the molar mass of SiO2? (Si = 28, O = 16)",
                new string[] { "44 g/mol", "60 g/mol", "448 g/mol" },
                "60 g/mol",
                new string[] { "Correct!", "SiO2 has 1 silicon and 2 oxygen: 28 + 16 + 16 = 60 g/mol." },
                new string[] { "Nice try! But unfortunately your answer is wrong!", "SiO2 has 1 silicon and 2 oxygen: 28 + 16 + 16 = 60 g/mol." },
                "Clarence AI", elementerAIPrefab, Bg),
            () => StudentSays("SiO2 has 1 silicon and 2 oxygen. 28 + 16 + 16 = 60 g/mol.",
                              "That matches the note. 60 grams is exactly one mole of silicon dioxide."),

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
                StudentSays("It worked! Clear crystals are settling into the mold!");
            },

            () => ShowTextCard("Keeper Dan weighs out 60 grams on his scale and shapes the glass in the forge. He fits the new lens into the lighthouse..."),

            () => DanSays("Exactly 60 grams. Just like the note says.", "There she goes! The ships can see us again."),
            () => DanSays("Take this lead weight from my scale. I have plenty."),

            // ---------------- REPAIR MATERIAL: LEAD ----------------
            () => AiSays("*The Elementer beeps.*", "REPAIR MATERIAL REQUIRED: LEAD."),
            () => StudentSays("That's the weight you just gave me.", "*You place the lead into the Elementer.*"),
            () => AiSays("LEAD DETECTED.", "DATABASE RECALIBRATION STARTED.", "ASSISTANCE REQUIRED."),

            // ---------------- PTQs ----------------
            () => ShowPTQ("Chapter "+MissionBase.Instance.ChapterNumber +"| PTQ 1",
                "Oxygen's atomic mass is 16. What is the mass of 1 mole of oxygen atoms?",
                new string[] { "8 g", "16 g", "32 g" },
                "16 g",
                "For an element, molar mass equals its atomic mass, so 1 mole of oxygen atoms is 16 g!", Bg),
            () => ShowPTQ("Chapter "+MissionBase.Instance.ChapterNumber +"| PTQ 2",
                "What is the molar mass of H2O? (H = 1, O = 16)",
                new string[] { "17 g/mol", "18 g/mol", "32 g/mol" },
                "18 g/mol",
                "H2O has 2 hydrogen and 1 oxygen: 1 + 1 + 16 = 18 g/mol!", Bg),
            () => ShowPTQ("Chapter "+MissionBase.Instance.ChapterNumber +"| PTQ 3",
                "What is the molar mass of MgO? (Mg = 24, O = 16)",
                new string[] { "24 g/mol", "40 g/mol", "384 g/mol" },
                "40 g/mol",
                "Add the atoms: 24 + 16 = 40 g/mol!", Bg),

            // ---------------- RECALIBRATION DONE + ELEMENTS UNLOCKED ----------------
            () => AiSays("DATABASE RECALIBRATION COMPLETE.", "MOLAR MASS FILES RESTORED.", ElementLocks.Unlock(6)),

            // ---------------- CHAPTER END ----------------
            () => DanSays("Wherever you're headed, keep your light burning."),
            () => AiSays(studentFirstName + ", the lead boosted my tracker.",
                         "Signal strength: 60%. We're well past halfway."),
            () => StudentSays("He's still in the same place, right?"),
            () => AiSays("Yes. He can't leave. Every repair just brings us closer.",
                         "NEXT TRACE DETECTED. PORTAL CHARGING."),
            () => StudentSays("Just hang on, Professor.", "*The Elementer glows, ready for the next jump.*"),

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

            () => ShowTextCard("Chapter 6\n" + chapterTitle + "\nComplete",
                               "\n\nPress the screen to go back to the main menu..."),
        };
    }
}
