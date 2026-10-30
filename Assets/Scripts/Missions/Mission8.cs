using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Mission8 : MissionBase
{
    public override int ChapterNumber => 8;

    public override string RequiredMixResult => "Ammonia";
    public override string MixInstructions => "Make what the terraces need.\nNot sure how? Ask Clarence AI for a hint.";

    private const string chapterTitle = "The Yellow Terraces";

    [Header("Chapter 8 only")]
    public GameObject talaPrefab;          // Elder Tala
    public Sprite terraceBackground;      // misty highland terraces at dawn (empty = forest night)

    private List<Action> steps;

    private Sprite Bg { get { return terraceBackground != null ? terraceBackground : forestNightBackground; } }

    private void StudentSays(params string[] lines) { Say(studentFirstName, emptyPrefab, Bg, lines); }
    private void TalaSays(params string[] lines) { Say("Elder Tala", talaPrefab, Bg, lines); }
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
            Debug.Log("End of mission 8");
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
            () => { StartCoroutine(WaitScene()); StudentSays("Okay... that one was rough."); },
            () => AiSays("Your readings are steady. Let me scan the area.",
                         "No sign of the professor yet, but the trace is faint here.",
                         "Let's find out where we are."),
            () => StudentSays("Hello? Sorry to bother you."),
            () => TalaSays("Oh! You came from the mist. Where is your caravan?"),
            () => StudentSays("I don't have one. I'm a traveler. Lost, honestly."),
            () => TalaSays("Lost travelers usually mean trouble. But you have kind eyes.",
                           "Look at these leaves."),
            () => StudentSays("They're yellow. Is it a disease?"),
            () => TalaSays("No. The soil is tired. It has run out of nitrogen.",
                           "My grandmother had a sharp-smelling water that turned yellow fields green again.",
                           "She never taught me how to make it. I only remember the smell, and that it needed nitrogen and hydrogen."),
            () => AiSays("PARTIAL MATCH FOUND."),
            () => StudentSays("Sharp-smelling, nitrogen and hydrogen... That's ammonia.", "I've made it before. With this.", "*You hold up the Elementer.*"),
            () => TalaSays("Then please, try. Each terrace needs exactly 2 moles, my grandmother said.",
                           "Nitrogen is all around us in the air. But hydrogen is the hard part.",
                           "We only gather a little each day from the hot spring vents.",
                           "So I must know how much hydrogen to set aside. In grams."),
            () => StudentSays("I know the reaction. N2 + 3H2 -> 2NH3.", "*You open the Elementer.*"),

            // ---------------- THE STORY TEACHES: mole ratios, then moles to grams ----------------
            () => AiSays("Good. It's balanced.",
                         "Last time, the big numbers in front of a formula told us how many crates of atoms we had.",
                         "They also tell us how many moles react. That's called the mole ratio."),
            () => StudentSays("Mole ratio? Like a recipe?"),
            () => AiSays("Exactly like a recipe.",
                         "Say a sandwich is 2 bread + 1 cheese. For 2 sandwiches, you need 4 bread and 2 cheese.",
                         "The recipe doesn't change. You just make more of it."),
            () => StudentSays("So N2 + 3H2 -> 2NH3 reads as...", "1 mole of N2, plus 3 moles of H2, makes 2 moles of NH3."),
            () => AiSays("Yes. 3 moles of hydrogen for every 2 moles of ammonia. Double the recipe and you get 6 hydrogen for 4 ammonia."),
            () => StudentSays("Okay. But Elder Tala needs grams, not moles."),
            () => AiSays("Then do the reverse of last chapter. Moles to grams means multiply by the molar mass.",
                         "Example: 5 moles of carbon, and carbon is 12 g/mol. 5 x 12 = 60 grams."),
            () => StudentSays("Divide for grams to moles. Multiply for moles to grams. Got it."),
            () => TalaSays("I follow none of that. But tell me what you've learned."),

            // ---------------- MCQs ----------------
            () => ShowMCQ("Chapter "+MissionBase.Instance.ChapterNumber +"| MCQ 1",
                "What do the big numbers (coefficients) in a balanced equation tell you?",
                new string[] { "The mole ratio of the substances", "The mass in grams of each substance", "The size of each atom" },
                "The mole ratio of the substances",
                new string[] { "Correct!", "Coefficients tell you how many moles react and form." },
                new string[] { "Nice try! But unfortunately your answer is wrong!", "Coefficients tell you how many moles react and form." },
                "Clarence AI", elementerAIPrefab, Bg),
            () => StudentSays("They tell me how many moles react and form."),
            () => TalaSays("Then show me. If I have 3 moles of hydrogen, how much ammonia will I get?"),

            () => ShowMCQ("Chapter "+MissionBase.Instance.ChapterNumber +"| MCQ 2",
                "In N2 + 3H2 -> 2NH3, how many moles of NH3 form from 3 moles of H2?",
                new string[] { "2 mol", "3 mol", "6 mol" },
                "2 mol",
                new string[] { "Correct!", "The ratio is 3 H2 to 2 NH3, so 3 moles of H2 makes 2 moles of NH3." },
                new string[] { "Nice try! But unfortunately your answer is wrong!", "The ratio is 3 H2 to 2 NH3, so 3 moles of H2 makes 2 moles of NH3." },
                "Clarence AI", elementerAIPrefab, Bg),
            () => StudentSays("The ratio is 3 H2 to 2 NH3. So 3 moles of hydrogen makes 2 moles of ammonia."),
            () => TalaSays("Good. Now my terraces. I need 2 moles of ammonia.", "How much hydrogen should I save, in grams?"),

            () => ShowMCQ("Chapter "+MissionBase.Instance.ChapterNumber +"| MCQ 3",
                "How many grams of H2 are needed to make 2 moles of NH3? (H2 = 2 g/mol)",
                new string[] { "3 g", "6 g", "12 g" },
                "6 g",
                new string[] { "Correct!", "2 mol of NH3 needs 3 mol of H2. Then 3 mol x 2 g/mol = 6 g." },
                new string[] { "Nice try! But unfortunately your answer is wrong!", "2 mol of NH3 needs 3 mol of H2. Then 3 mol x 2 g/mol = 6 g." },
                "Clarence AI", elementerAIPrefab, Bg),
            () => StudentSays("2 moles of NH3 needs 3 moles of H2. Then 3 x 2 g/mol = 6 g.", "Six grams of hydrogen. That's all we need."),

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
                StudentSays("It worked! That's 2 moles of ammonia!");
            },

            () => ShowTextCard("Elder Tala pours the ammonia into the water channel. It flows down the terraces, step by step. Time passes, and the yellow leaves turn green..."),

            () => TalaSays("Look at them! Green again, all the way down.",
                           "You saved this year's harvest.",
                           "Take this zinc ingot. We use it to protect our tools from rust."),

            // ---------------- REPAIR MATERIAL: ZINC ----------------
            () => AiSays("*The Elementer beeps.*", "REPAIR MATERIAL REQUIRED: ZINC."),
            () => StudentSays("That's the ingot you just gave me.", "*You place the zinc into the Elementer.*"),
            () => AiSays("ZINC DETECTED.", "DATABASE RECALIBRATION STARTED.", "ASSISTANCE REQUIRED."),

            // ---------------- PTQs ----------------
            () => ShowPTQ("Chapter "+MissionBase.Instance.ChapterNumber +"| PTQ 1",
                "In 2H2 + O2 -> 2H2O, how many moles of H2 react with 1 mole of O2?",
                new string[] { "1 mol", "2 mol", "3 mol" },
                "2 mol",
                "The coefficients are 2, 1, 2, so 2 moles of H2 react with 1 mole of O2!", Bg),
            () => ShowPTQ("Chapter "+MissionBase.Instance.ChapterNumber +"| PTQ 2",
                "2H2O -> 2H2 + O2. How many moles of O2 form when 4 moles of H2O break down?",
                new string[] { "2 mol", "4 mol", "8 mol" },
                "2 mol",
                "The ratio is 2 H2O to 1 O2, so 4 moles of H2O makes 2 moles of O2!", Bg),
            () => ShowPTQ("Chapter "+MissionBase.Instance.ChapterNumber +"| PTQ 3",
                "In 2H2 + O2 -> 2H2O, how many grams of O2 are needed to react with 4 moles of H2? (O = 16)",
                new string[] { "32 g", "64 g", "128 g" },
                "64 g",
                "4 mol H2 needs 2 mol O2. O2 is 32 g/mol, so 2 x 32 = 64 g!", Bg),

            // ---------------- RECALIBRATION DONE + ELEMENTS UNLOCKED ----------------
            () => AiSays("DATABASE RECALIBRATION COMPLETE.", "MOLE RATIO FILES RESTORED.", ElementLocks.Unlock(8)),

            // ---------------- CHAPTER END ----------------
            () => TalaSays("The terraces will remember you. Safe roads, traveler."),
            () => AiSays(studentFirstName + ", the zinc cleared up a lot of noise.",
                         "Signal strength: 80%. And now I can see more than before."),
            () => StudentSays("What do you see?"),
            () => AiSays("His signal is weak because something is surrounding him. The area is very cold, and it looks closed in."),
            () => StudentSays("Like he's locked in?"),
            () => AiSays("Maybe. Stuck, or held. I can't tell yet, but I know where to aim the next portal.",
                         "NEXT TRACE DETECTED. PORTAL CHARGING."),
            () => StudentSays("Almost there, Professor. Hold on.", "*The Elementer glows, ready for the next jump.*"),

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

            () => ShowTextCard("Chapter 8\n" + chapterTitle + "\nComplete",
                               "\n\nPress the screen to go back to the main menu..."),
        };
    }
}
