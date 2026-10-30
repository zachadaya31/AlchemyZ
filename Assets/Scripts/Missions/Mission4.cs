using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Mission4 : MissionBase
{
    public override int ChapterNumber => 4;

    public override string RequiredMixResult => "Magnesium Oxide";
    public override string MixInstructions => "Create Magnesium Oxide";

    private const string chapterTitle = "Signal in the Fog";

    [Header("Chapter 4 only")]
    public GameObject kaiPrefab;          // Ranger Kai
    public Sprite outpostBackground;      // foggy mountain outpost (empty = forest night)

    private List<Action> steps;

    private Sprite Bg { get { return outpostBackground != null ? outpostBackground : forestNightBackground; } }

    private void StudentSays(params string[] lines) { Say(studentFirstName, emptyPrefab, Bg, lines); }
    private void KaiSays(params string[] lines) { Say("Ranger Kai", kaiPrefab, Bg, lines); }
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
            Debug.Log("End of mission 4");
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
            () => { StartCoroutine(WaitScene()); StudentSays("I can't see past my own hands...", "Where are we?"); },
            () => AiSays("Stay close, " + studentFirstName + ".", "The professor's signal is faint here, but it's real."),
            () => KaiSays("Stop right there! Who goes there?"),
            () => StudentSays("Just a traveler. Lost, mostly."),
            () => KaiSays("Then you're not the only one tonight.",
                          "A group of travelers walked into this fog hours ago, and my flares ran out.",
                          "Without a bright light, they'll walk right off the cliff."),
            () => StudentSays("How bright does it need to be?"),
            () => KaiSays("Bright white. Bright enough to cut through all of this."),
            () => StudentSays("Magnesium burns exactly like that. I've seen it in the lab.",
                              "Give me a moment. I think I can make one.", "*You open the Elementer.*"),

            // ---------------- THE STORY TEACHES: balancing, one step at a time ----------------
            () => AiSays("Magnesium plus oxygen makes magnesium oxide. That's your flare.",
                         "Written out, it looks like this: Mg + O2 -> MgO.",
                         "Oxygen travels in pairs, so O2 means 2 oxygen atoms stuck together."),
            () => StudentSays("So on the left I have 2 oxygen atoms.", "On the right, MgO has... only 1."),
            () => AiSays("That's the problem.",
                         "Atoms can't be created or destroyed, so whatever goes in must come out.",
                         "If the equation doesn't match, the mix fizzles."),
            () => StudentSays("Then I have to even it out. How?"),
            () => AiSays("With a big number in front of a formula. It's called a coefficient.",
                         "Think of ordering crates. 2MgO means 2 Mg and 2 O.",
                         "It multiplies everything in that formula. 2H2O would be 4 hydrogen and 2 oxygen."),
            () => StudentSays("So 2MgO gives me 2 oxygen on the right. That matches the 2 on the left!"),
            () => AiSays("Good. Now count the magnesium."),
            () => StudentSays("Left side has 1 Mg. Right side has 2.", "So I need 2Mg on the left."),
            () => AiSays("2Mg + O2 -> 2MgO. Count one more time."),
            () => StudentSays("2 Mg and 2 O on the left. 2 Mg and 2 O on the right.", "It's balanced!"),
            () => AiSays("One rule: only change the big numbers.",
                         "Change a small number, like turning MgO into MgO2, and you've made a different substance."),
            () => StudentSays("Big numbers yes, small numbers never. Got it."),
            () => KaiSays("I only understood the counting part.", "But if it makes a flare, I'm listening. Humor me."),

            // ---------------- MCQs (answerable from what we just taught) ----------------
            () => ShowMCQ("Chapter "+MissionBase.Instance.ChapterNumber +"| MCQ 1",
                "Why can't one of the oxygen atoms just disappear?",
                new string[] { "Because atoms can't be created or destroyed", "Because the flare would be too dim", "Because oxygen is too heavy" },
                "Because atoms can't be created or destroyed",
                new string[] { "Correct!", "Atoms can't be created or destroyed, so both sides must match." },
                new string[] { "Nice try! But unfortunately your answer is wrong!", "Atoms can't be created or destroyed, so both sides must match." },
                "Clarence AI", elementerAIPrefab, Bg),
            () => StudentSays("Right. Nothing appears out of thin air, and nothing just vanishes."),
            () => KaiSays("Fair enough. Then count for me.", "How many oxygen atoms are you starting with?"),

            () => ShowMCQ("Chapter "+MissionBase.Instance.ChapterNumber +"| MCQ 2",
                "In Mg + O2 -> MgO, how many oxygen atoms are on the left side?",
                new string[] { "1", "2", "3" },
                "2",
                new string[] { "Correct!", "O2 means 2 oxygen atoms joined together." },
                new string[] { "Nice try! But unfortunately your answer is wrong!", "O2 means 2 oxygen atoms joined together." },
                "Clarence AI", elementerAIPrefab, Bg),
            () => StudentSays("Two going in, but only one coming out.", "That's why I needed the 2 in front of MgO."),
            () => KaiSays("Then tell me what to light. Which one is right?"),

            () => ShowMCQ("Chapter "+MissionBase.Instance.ChapterNumber +"| MCQ 3",
                "Which equation is balanced?",
                new string[] { "Mg + O2 -> MgO", "2Mg + O2 -> 2MgO", "Mg + O2 -> MgO2" },
                "2Mg + O2 -> 2MgO",
                new string[] { "Correct!", "2Mg + O2 -> 2MgO has 2 Mg and 2 O on both sides." },
                new string[] { "Nice try! But unfortunately your answer is wrong!", "2Mg + O2 -> 2MgO has 2 Mg and 2 O on both sides." },
                "Clarence AI", elementerAIPrefab, Bg),
            () => StudentSays("2Mg + O2 -> 2MgO. Two magnesium, two oxygen, on both sides.", "That's the one. Let's make it."),

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
                StudentSays("It worked! Kai, the flare is ready!");
            },

            () => ShowTextCard("Ranger Kai raises the flare and lights it. A blinding white light tears through the fog..."),

            () => KaiSays("There! I see them, they're coming down the path!",
                          "You saved them, traveler. I owe you more than I can repay.",
                          "Take this silver mirror. It was my grandfather's. It never lost its shine."),

            // ---------------- REPAIR MATERIAL: SILVER ----------------
            () => AiSays("*The Elementer beeps.*", "REPAIR MATERIAL REQUIRED: SILVER."),
            () => StudentSays("Kai, the mirror you just gave me...", "*You place the silver into the Elementer.*"),
            () => AiSays("SILVER DETECTED.", "DATABASE RECALIBRATION STARTED.", "ASSISTANCE REQUIRED."),

            // ---------------- PTQs ----------------
            () => ShowPTQ("Chapter "+MissionBase.Instance.ChapterNumber +"| PTQ 1",
                "The left side of a balanced equation has 4 hydrogen atoms. How many hydrogen atoms are on the right side?",
                new string[] { "2", "4", "8" },
                "4",
                "A balanced equation has the same number of each atom on both sides, so there are 4!", Bg),
            () => ShowPTQ("Chapter "+MissionBase.Instance.ChapterNumber +"| PTQ 2",
                "How many hydrogen atoms are in 2H2O?",
                new string[] { "2", "4", "6" },
                "4",
                "The 2 in front multiplies the whole formula: 2 x 2 hydrogen atoms = 4!", Bg),
            () => ShowPTQ("Chapter "+MissionBase.Instance.ChapterNumber +"| PTQ 3",
                "Which equation for forming water is balanced?",
                new string[] { "H2 + O2 -> H2O", "2H2 + O2 -> 2H2O", "H2 + O2 -> H2O2" },
                "2H2 + O2 -> 2H2O",
                "2H2 + O2 -> 2H2O has 4 hydrogen and 2 oxygen on both sides!", Bg),

            // ---------------- RECALIBRATION DONE + ELEMENTS UNLOCKED ----------------
            () => AiSays("DATABASE RECALIBRATION COMPLETE.", "BALANCING FILES RESTORED.", ElementLocks.Unlock(4)),

            // ---------------- CHAPTER END ----------------
            () => KaiSays("Whatever you're chasing out there, I hope you find it.",
                          "The fog isn't the only dark thing in these mountains."),
            () => AiSays(studentFirstName + ", the silver sharpened my signal tracker.",
                         "The professor isn't moving. Without an Elementer, he can't leave wherever he is."),
            () => StudentSays("So he's out there, waiting for us."),
            () => AiSays("Yes. My database still blurs his location, but every repair brings it into focus.",
                         "Signal strength: 40%. We're getting closer.",
                         "NEXT TRACE DETECTED. PORTAL CHARGING."),
            () => StudentSays("Hold on, Professor. We're coming.",
                              "*The Elementer glows, ready for the next jump.*"),

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

            () => ShowTextCard("Chapter 4\n" + chapterTitle + "\nComplete",
                               "\n\nPress the screen to go back to the main menu..."),
        };
    }
}