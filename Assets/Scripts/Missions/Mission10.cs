using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Mission10 : MissionBase
{
    public override int ChapterNumber => 10;

    public override string RequiredMixResult => mixNow;
    public override string MixInstructions => hintNow;

    // first mix = Calcium Oxide, second mix = Potassium Chloride (changed right before the second Elementer opens)
    private string mixNow = "Calcium Oxide";
    private string hintNow = "Task: Create Calcium Oxide";

    private const string chapterTitle = "The Last Signal";

    [Header("Chapter 10 only")]
    public GameObject zachPrefab;          // Professor Zach
    public Sprite hellBackground;
    public GameObject vexPrefab;        // Warden Vex (demon)
    public GameObject mikkoPrefab;      // Teacher Mikko
    public Sprite labBackground;        // the laboratory at home (empty = same as hell background)

    private List<Action> steps;

    private Sprite Bg { get { return hellBackground != null ? hellBackground : forestNightBackground; } }

    private Sprite LabBg { get { return labBackground != null ? labBackground : Bg; } }
    private void VexSays(params string[] lines) { Say("Warden Vex", vexPrefab, Bg, lines); }
    private void MikkoSays(params string[] lines) { Say("Teacher Mikko", mikkoPrefab, LabBg, lines); }
    private void LabStudentSays(params string[] lines) { Say(studentFirstName, emptyPrefab, LabBg, lines); }
    private void LabAiSays(params string[] lines) { Say("Clarence AI", elementerAIPrefab, LabBg, lines); }

    private void StudentSays(params string[] lines) { Say(studentFirstName, emptyPrefab, Bg, lines); }
    private void ZachSays(params string[] lines) { Say("Professor Zach", zachPrefab, Bg, lines); }
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
            Debug.Log("End of mission 10");
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
            () => { StartCoroutine(WaitScene()); StudentSays("It's like standing inside a furnace."); },
            () => AiSays("Your readings are steady, but the heat is rising. My tracker is spiking. He's close.",
                         "Stay low. Something is moving out there."),

            // ---------------- MOMENT 1: SAVE THE PROFESSOR ----------------
            () => ShowTextCard("Across a stone bridge, an iron cage hangs from a chain above a river of lava. A figure sits inside. Two tall horned demons stand guard..."),
            () => StudentSays("Professor Zach!"),
            () => ZachSays("Shh! Apprentice? Is that really you?"),
            () => StudentSays("It's me! We found you!"),
            () => ZachSays("You carried a broken Elementer through time. Remarkable. But keep your voice down."),
            () => StudentSays("What happened? Who are they?"),
            () => ZachSays("The demons of this realm. They saw the portal tear open and thought I was a sorcerer.",
                           "They locked me in this cage and took my tools."),
            () => VexSays("Quiet in there, sorcerer! Your master will hear of this."),
            () => StudentSays("I can't fight two demons. And that lock looks too strong to break."),
            () => ZachSays("Then do not fight. Think.",
                           "These demons live in red gloom. Their eyes are not made for bright white light.",
                           "Quicklime. Calcium oxide. Heat it hard and it blazes white. Theaters called it limelight."),
            () => StudentSays("Calcium and oxygen. Let me open the Elementer.", "*You open the Elementer.*"),

            // ---------------- THE STORY RECAPS: naming, balancing, reaction type ----------------
            () => AiSays("Equation: Ca + O2 -> CaO.", "STATUS: NOT BALANCED."),
            () => ZachSays("Before you build it, show me what you've learned. A teacher has to know.",
                           "First, what is this compound called?"),
            () => StudentSays("Metal first, and the nonmetal ends in -ide. Calcium, and oxygen becomes oxide.", "Calcium oxide."),
            () => ZachSays("Good. The Elementer says it's not balanced. Why?"),
            () => StudentSays("Left side has 2 oxygen atoms. Right side has only 1."),
            () => AiSays("Remember: change only the big numbers, never the small ones.",
                         "2CaO fixes the oxygen. Then 2Ca on the left fixes the calcium."),
            () => StudentSays("2Ca + O2 -> 2CaO. 2 calcium and 2 oxygen on both sides."),
            () => ZachSays("And what kind of reaction is it?"),
            () => StudentSays("Two things join into one. That's a combination reaction."),
            () => ZachSays("Now prove it. Which equation is balanced?"),

            // ---------------- MCQ 1 ----------------
            () => ShowMCQ("Chapter "+MissionBase.Instance.ChapterNumber +"| MCQ 1",
                "Which equation is balanced?",
                new string[] { "Ca + O2 -> CaO", "2Ca + O2 -> 2CaO", "Ca + O2 -> CaO2" },
                "2Ca + O2 -> 2CaO",
                new string[] { "Correct!", "2Ca + O2 -> 2CaO has 2 Ca and 2 O on both sides." },
                new string[] { "Nice try! But unfortunately your answer is wrong!", "2Ca + O2 -> 2CaO has 2 Ca and 2 O on both sides." },
                "Clarence AI", elementerAIPrefab, Bg),
            () => ZachSays("My apprentice remembers. Now make the light.", "Cover your eyes when it blazes."),

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
                StudentSays("It worked!");
            },

            () => ShowTextCard("You hold the Elementer up and shield your eyes. A blinding white light floods the cavern..."),

            () => VexSays("My eyes! Who dares bring the sun into the Pit?"),
            () => ShowTextCard("The demons stumble away down the bridge, and one drops a ring of keys. You run across, grab the keys, and unlock the cage..."),
            () => StudentSays("Professor! Are you hurt?"),
            () => ZachSays("Only tired and very, very hot.", "You saved my life, apprentice."),
            () => ZachSays("This was my fault. I rushed the prototype and the core overloaded.", "I am sorry I dragged you into it."),
            () => StudentSays("You didn't drag me. I'm the one who followed."),
            () => ShowTextCard("A deep horn echoes across the cavern."),
            () => ZachSays("They will be back soon. Open the portal home. Quickly."),

            // ---------------- MOMENT 2: GO HOME ----------------
            () => AiSays("The portal ring flickers. Heat ripples off the black rock around it.",
                         "PORTAL HOME REQUIRES STABILIZER: POTASSIUM CHLORIDE, 2 MOLES.",
                         "WARNING: AMBIENT HEAT IS EXTREME. STABILIZER MUST BE A SOLID THAT HOLDS TOGETHER IN HEAT."),
            () => ZachSays("Then the type of solid matters. Tell me what the Elementer knows."),
            () => AiSays("There are three types of solids.",
                         "Ionic solids are made of positive and negative ions that pull on each other strongly. Table salt is one. They are hard and have high melting points.",
                         "Metallic solids are metal atoms sharing a sea of electrons. A copper wire is one. They conduct electricity and bend without breaking."),
            () => AiSays("Molecular solids are molecules held together weakly. Ice is one. They are soft and have low melting points."),
            () => StudentSays("Ionic, metallic, molecular.", "So the strong, hard one survives heat. The weak one melts."),
            () => ZachSays("Potassium chloride is made of positive and negative ions, held together strongly.", "What type of solid is it?"),

            // ---------------- MCQ 2 ----------------
            () => ShowMCQ("Chapter "+MissionBase.Instance.ChapterNumber +"| MCQ 2",
                "Potassium chloride is made of positive and negative ions held together by strong attraction. What type of solid is it?",
                new string[] { "Metallic", "Ionic", "Molecular" },
                "Ionic",
                new string[] { "Correct!", "Ions held together by strong attraction make an ionic solid, hard with a high melting point." },
                new string[] { "Nice try! But unfortunately your answer is wrong!", "Ions held together by strong attraction make an ionic solid, hard with a high melting point." },
                "Clarence AI", elementerAIPrefab, Bg),
            () => StudentSays("Ionic. Positive and negative ions, just like salt."),
            () => ZachSays("Good. Hard, with a high melting point. It will survive this heat."),
            () => ShowTextCard("A deep horn echoes across the cavern."),
            () => ZachSays("They are coming. Now the equation. Quickly."),
            () => StudentSays("*You open the Elementer.*"),
            () => AiSays("EQUATION: 2K + Cl2 -> 2KCl.", "STATUS: BALANCED. REACTION TYPE: COMBINATION.",
                         "Remember, the big numbers are mole ratios."),
            () => ZachSays("We need 2 moles of potassium chloride. How many moles of potassium must we gather?"),
            () => StudentSays("Let me read the ratio. 2K and 2KCl.", "Same number on both."),

            // ---------------- MCQ 3 ----------------
            () => ShowMCQ("Chapter "+MissionBase.Instance.ChapterNumber +"| MCQ 3",
                "In 2K + Cl2 -> 2KCl, how many moles of K are needed to make 2 moles of KCl?",
                new string[] { "4 mol", "1 mol", "2 mol" },
                "2 mol",
                new string[] { "Correct!", "The ratio is 2 K to 2 KCl, so 2 moles of KCl needs 2 moles of K." },
                new string[] { "Nice try! But unfortunately your answer is wrong!", "The ratio is 2 K to 2 KCl, so 2 moles of KCl needs 2 moles of K." },
                "Clarence AI", elementerAIPrefab, Bg),
            () => StudentSays("2 moles of potassium. And 1 mole of Cl2 goes with it."),
            () => ZachSays("Then convert to grams so the Elementer can measure it. Potassium is 39 g/mol."),
            () => StudentSays("2 mol x 39 g/mol = 78 g of potassium."),
            () => ZachSays("And the chlorine? It comes as Cl2. What does one mole of it weigh?"),
            () => AiSays("For a molecule, add every atom in the formula. Cl = 35.5 g/mol."),
            () => StudentSays("Two chlorine atoms. 2 x 35.5 = 71 g.", "So 78 g of potassium and 71 g of chlorine."),
            () => ZachSays("Exactly. Go. I hear them coming."),

            // ---------------- SECOND ELEMENTER MIX ----------------
            () =>
            {
                mixNow = "Potassium Chloride";
                hintNow = "Task: Create Potassium Chloride";
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

            () => ShowTextCard("The stabilizer slot glows steady blue. The ring stops flickering. Far down the bridge, the demons shake off the glare and charge toward you..."),

            // ---------------- REPAIR MATERIAL: PLATINUM ----------------
            () => AiSays("STABILIZER LOADED.", "PORTAL CONTROL FILES CORRUPTED. DATABASE INCOMPLETE.", "REPAIR MATERIAL REQUIRED: PLATINUM."),
            () => StudentSays("Platinum? Where would I find that in Hell?"),
            () => ZachSays("*Professor Zach rolls up his sleeve and pulls out a thin wire loop.*",
                           "My platinum wire loop. I use it for flame tests. They searched my pockets, but never my sleeve."),
            () => StudentSays("Professor, that's what the Elementer needs."),
            () => ZachSays("Then it's yours. A small thanks for coming all this way."),
            () => StudentSays("*You place the platinum into the Elementer.*"),
            () => AiSays("PLATINUM DETECTED.", "DATABASE RECALIBRATION STARTED.", "ASSISTANCE REQUIRED."),

            // ---------------- PTQs ----------------
            () => ShowPTQ("Chapter "+MissionBase.Instance.ChapterNumber +"| PTQ 1",
                "Which equation for forming zinc oxide is balanced?",
                new string[] { "Zn + O2 -> ZnO", "2Zn + O2 -> 2ZnO", "Zn + O2 -> ZnO2" },
                "2Zn + O2 -> 2ZnO",
                "2Zn + O2 -> 2ZnO has 2 Zn and 2 O on both sides!", Bg),
            () => ShowPTQ("Chapter "+MissionBase.Instance.ChapterNumber +"| PTQ 2",
                "A copper wire conducts electricity and bends without breaking. What type of solid is copper?",
                new string[] { "Metallic", "Ionic", "Molecular" },
                "Metallic",
                "Metals conduct electricity and bend without breaking, so copper is a metallic solid!", Bg),
            () => ShowPTQ("Chapter "+MissionBase.Instance.ChapterNumber +"| PTQ 3",
                "How many grams are in 2 moles of CO2? (C = 12, O = 16)",
                new string[] { "44 g", "176 g", "88 g" },
                "88 g",
                "CO2 is 12 + 16 + 16 = 44 g/mol, so 2 moles is 2 x 44 = 88 g!", Bg),

            // ---------------- RECALIBRATION DONE + ELEMENTS UNLOCKED ----------------
            () => AiSays("DATABASE RECALIBRATION COMPLETE.", "ALL FILES RESTORED.", ElementLocks.Unlock(10)),

            // ---------------- CHAPTER END ----------------
            () => AiSays("Hello, original."),
            () => ZachSays("Well. My backup survived, and it's a little cheeky."),
            () => AiSays(studentFirstName + " repaired every file. Signal strength: 100%. All systems ready.",
                         "PORTAL HOME READY."),
            () => ShowTextCard("The demons reach the edge of the platform, just as the ring blazes..."),
            () => ZachSays("Apprentice, I can't thank you enough. You solved every problem along the way."),
            () => StudentSays("I had good teachers. And a good machine."),
            () => StudentSays("*You and the Professor leap through together.*"),

            // ---------------- ENDING VIDEO (clip on VideoPlayer2, slot 2): the jump home ----------------
            () =>
            {
                dialogueObject.SetActive(false);
                backButton.interactable = false;
                nextButton.interactable = false;

                videoPlayerObject2.SetActive(true);
                videoPlayer2.Play();
                videoPlayer2.loopPointReached += OnVideoFinished;
            },

            // ---------------- BACK IN THE LAB ----------------
            () => MikkoSays("There you are. Professor Zach, " + studentFirstName + ", class is about to start.",
                            "You two look like you've been through a furnace."),
            () => Say("Professor Zach", zachPrefab, LabBg, "Something like that."),
            () => LabAiSays("WELCOME HOME."),
            () => LabStudentSays("Next time, I'm bringing a fan."),

            () => ShowTextCard("Chapter 10\n" + chapterTitle + "\nStory Complete",
                               "\n\nPress the screen to go back to the main menu..."),
        };
    }
}
