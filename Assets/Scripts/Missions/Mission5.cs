using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Mission5 : MissionBase
{
    public override int ChapterNumber => 5;

    public override string RequiredMixResult => "Carbon Dioxide";
    public override string MixInstructions => "Put out the fire.\nNot sure how? Ask Clarence AI for a hint.";

    private const string chapterTitle = "Smothering the Flames";

    [Header("Chapter 5 only")]
    public GameObject naraPrefab;          // Farmer Nara
    public Sprite villageBackground;      // burning village at night (empty = forest night)

    private List<Action> steps;

    private Sprite Bg { get { return villageBackground != null ? villageBackground : forestNightBackground; } }

    private void StudentSays(params string[] lines) { Say(studentFirstName, emptyPrefab, Bg, lines); }
    private void NaraSays(params string[] lines) { Say("Farmer Nara", naraPrefab, Bg, lines); }
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
            Debug.Log("End of mission 5");
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
            () => { StartCoroutine(WaitScene()); StudentSays("That storehouse is on fire!"); },
            () => AiSays("Stay alert, " + studentFirstName + ".", "Someone needs help."),
            () => NaraSays("Help! My storehouse is on fire!", "All my lamp oil is inside!"),
            () => StudentSays("Get some water!"),
            () => NaraSays("No! Oil floats on water. Water will only spread the flames!"),
            () => StudentSays("Then we have to smother it.", "Carbon dioxide can do that. It settles over a fire and cuts off its oxygen."),
            () => StudentSays("I can make it from carbon and oxygen.", "*You open the Elementer.*"),

            // ---------------- THE STORY TEACHES: combination and decomposition ----------------
            () => AiSays("Here is the equation: C + O2 -> CO2.",
                         "This is a combination reaction.",
                         "Combination means two or more substances join to make one new substance."),
            () => StudentSays("Carbon and oxygen go in. One carbon dioxide comes out.", "Like snapping blocks together."),
            () => AiSays("Exactly. The opposite is called decomposition.",
                         "One substance breaks apart into two or more simpler ones, like a tower of blocks falling over.",
                         "Water is a good example: 2H2O -> 2H2 + O2."),
            () => StudentSays("One thing on the left, two things on the right. It split apart."),
            () => AiSays("Here is a quick trick.",
                         "One substance on the RIGHT side means combination.",
                         "One substance on the LEFT side means decomposition."),
            () => StudentSays("Joining makes one thing. Breaking apart makes more."),
            () => NaraSays("I don't understand your strange talk, but hurry!", "What kind of reaction are you making?"),

            // ---------------- MCQs ----------------
            () => ShowMCQ("Chapter "+MissionBase.Instance.ChapterNumber +"| MCQ 1",
                "What happens in a combination reaction?",
                new string[] { "Two or more substances join to make one product", "One substance breaks into simpler substances", "Atoms are destroyed" },
                "Two or more substances join to make one product",
                new string[] { "Correct!", "In a combination reaction, things join together to make one new substance." },
                new string[] { "Nice try! But unfortunately your answer is wrong!", "In a combination reaction, things join together to make one new substance." },
                "Clarence AI", elementerAIPrefab, Bg),
            () => StudentSays("Things join together into one new substance."),
            () => NaraSays("Good. Which of these is the same kind of reaction?"),

            () => ShowMCQ("Chapter "+MissionBase.Instance.ChapterNumber +"| MCQ 2",
                "Which reaction is a combination reaction?",
                new string[] { "2H2O -> 2H2 + O2", "C + O2 -> CO2", "2MgO -> 2Mg + O2" },
                "C + O2 -> CO2",
                new string[] { "Correct!", "C + O2 -> CO2 has two substances on the left and only one on the right." },
                new string[] { "Nice try! But unfortunately your answer is wrong!", "C + O2 -> CO2 has two substances on the left and only one on the right." },
                "Clarence AI", elementerAIPrefab, Bg),
            () => StudentSays("C + O2 -> CO2. Two on the left, one on the right."),
            () => NaraSays("And the opposite kind?"),

            () => ShowMCQ("Chapter "+MissionBase.Instance.ChapterNumber +"| MCQ 3",
                "What happens in a decomposition reaction?",
                new string[] { "One substance breaks apart into simpler substances", "Two substances join into one", "Atoms disappear" },
                "One substance breaks apart into simpler substances",
                new string[] { "Correct!", "In decomposition, one substance breaks apart into two or more simpler ones." },
                new string[] { "Nice try! But unfortunately your answer is wrong!", "In decomposition, one substance breaks apart into two or more simpler ones." },
                "Clarence AI", elementerAIPrefab, Bg),
            () => StudentSays("One thing breaks apart into smaller pieces.", "Okay. Time to put out that fire."),

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
                StudentSays("It worked! The gas is pouring out!");
            },

            () => ShowTextCard("You release the gas over the storehouse. Slowly, the flames die out..."),

            () => NaraSays("The fire's out! You saved my harvest!",
                           "I don't have much, but please take this tin pail. It's all I can give."),

            // ---------------- REPAIR MATERIAL: TIN ----------------
            () => AiSays("*The Elementer beeps.*", "REPAIR MATERIAL REQUIRED: TIN."),
            () => StudentSays("The pail you just gave me...", "*You place the tin into the Elementer.*"),
            () => AiSays("TIN DETECTED.", "DATABASE RECALIBRATION STARTED.", "ASSISTANCE REQUIRED."),

            // ---------------- PTQs ----------------
            () => ShowPTQ("Chapter "+MissionBase.Instance.ChapterNumber +"| PTQ 1",
                "2Mg + O2 -> 2MgO. What type of reaction is this?",
                new string[] { "Decomposition", "Combination", "Neither" },
                "Combination",
                "Two substances join to make one product, so it is a combination reaction!", Bg),
            () => ShowPTQ("Chapter "+MissionBase.Instance.ChapterNumber +"| PTQ 2",
                "Which reaction has only one substance on the right side?",
                new string[] { "2MgO -> 2Mg + O2", "2H2 + O2 -> 2H2O", "2H2O -> 2H2 + O2" },
                "2H2 + O2 -> 2H2O",
                "One substance on the right side means combination!", Bg),
            () => ShowPTQ("Chapter "+MissionBase.Instance.ChapterNumber +"| PTQ 3",
                "Which reaction is a decomposition reaction?",
                new string[] { "2H2O -> 2H2 + O2", "2H2 + O2 -> 2H2O", "C + O2 -> CO2" },
                "2H2O -> 2H2 + O2",
                "One substance on the left breaks apart into two, so it is decomposition!", Bg),

            // ---------------- RECALIBRATION DONE + ELEMENTS UNLOCKED ----------------
            () => AiSays("DATABASE RECALIBRATION COMPLETE.", "REACTION TYPE FILES RESTORED.", ElementLocks.Unlock(5)),

            // ---------------- CHAPTER END ----------------
            () => NaraSays("If you ever pass through here again, there will be a warm meal waiting for you."),
            () => AiSays(studentFirstName + ", the tin boosted my tracker.",
                         "Signal strength: 50%. We're halfway there."),
            () => StudentSays("And he hasn't moved?"),
            () => AiSays("He can't. No Elementer, no way out.",
                         "He's in one place, and we're getting closer.",
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

            () => ShowTextCard("Chapter 5\n" + chapterTitle + "\nComplete",
                               "\n\nPress the screen to go back to the main menu..."),
        };
    }
}
