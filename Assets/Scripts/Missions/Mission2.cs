using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// CHAPTER 2: Heneral Tuna's camp, make Salt (Sodium + Chlorine), then the Iron / PTQ recalibration.
// Everything shared (fields, Awake/Start, MCQ, PTQ, database) is in MissionBase.cs.
// The story below is a list of steps. nextScene() just runs the next step each time it is called.
public class Mission2 : MissionBase
{
    public override int ChapterNumber => 2;

    // The Elementer scene only moves the story when THIS result is tapped.
    // Must match the Sodium Chloride recipe's resultName (or its result prefab name). Spaces and capitals are ignored.
    public override string RequiredMixResult => "Sodium Chloride";

    // text shown on top of the Elementer scene during this chapter's mix
    public override string MixInstructions => "Select Sodium (Na) and Chlorine (Cl), then press Mix to make Salt.";

    private const string chapterTitle = "A Pinch of Salt";   // change this if you pick another title

    [Header("Chapter 2 only")]
    public GameObject tunaPrefab;      // Heneral Tuna character
    public Sprite campBackground;      // leave empty to use the forest background for now

    private List<Action> steps;

    private Sprite Camp { get { return campBackground != null ? campBackground : forestBackground; } }

    // little shortcuts so the story below reads like the script
    private void StudentSays(params string[] lines) { Say(studentFirstName, emptyPrefab, Camp, lines); }
    private void TunaSays(params string[] lines) { Say("Heneral Tuna", tunaPrefab, Camp, lines); }
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
            Debug.Log("End of mission 2");
            FinishChapter(); // unlocks chapter 3 in the database, then goes back to the main menu
        }
    }

    private void BuildSteps()
    {
        steps = new List<Action>
        {
            // ---------------- 1: WAKING UP + MEETING HENERAL TUNA ----------------
            () => { StartCoroutine(WaitScene()); StudentSays("Ahh... much better.", "Now... where did Professor Zach go?"); },
            () => TunaSays("Hey! You there!"),
            () => StudentSays("Huh?!"),
            () => TunaSays("What are you doing in my camp?"),
            () => StudentSays("Your camp?"),
            () => TunaSays("Yes. How did you get here? Who are you?"),
            () => StudentSays("I'm... a student. I was separated from my professor."),
            () => TunaSays("Your professor?"),
            () => StudentSays("Yes. We were together before I ended up here."),
            () => TunaSays("And what is that strange machine you're carrying?"),
            () => StudentSays("It's called the Elementer."),
            () => TunaSays("The Elementer?"),
            () => StudentSays("It's a machine that can create things by combining elements."),
            () => TunaSays("Elements? I don't understand."),
            () => StudentSays("It's... kind of complicated."),
            () => TunaSays("Well, perhaps you can use that strange machine to help us."),
            () => StudentSays("What do you need?"),
            () => TunaSays("We're preparing food for the camp, but we've run out of salt."),
            () => StudentSays("Salt?"),
            () => TunaSays("Yes. Without it, our food will be bland."),
            () => StudentSays("Maybe the Elementer can make some."),
            () => TunaSays("Then what are you waiting for?"),
            () => StudentSays("If I'm going to make salt, I should probably figure out which elements I need."),

            // ---------------- MCQ 1 (Sodium) ----------------
            () => ShowMCQ("MCQ 1",
                "Which element has the chemical symbol Na?",
                new string[] { "Nitrogen", "Sodium", "Neon" },
                "Sodium",
                new string[] { "Correct!", "Na is the symbol for Sodium." },
                new string[] { "Nice try! But unfortunately your answer is wrong!", "Na is the symbol for Sodium." },
                "Clarence AI", elementerAIPrefab, Camp),
            () => StudentSays("Sodium.", "And I need one more element..."),

            // ---------------- MCQ 2 (Chlorine) ----------------
            () => ShowMCQ("MCQ 2",
                "Which element has the chemical symbol Cl?",
                new string[] { "Calcium", "Carbon", "Chlorine" },
                "Chlorine",
                new string[] { "Correct!", "Cl is the symbol for Chlorine." },
                new string[] { "Nice try! But unfortunately your answer is wrong!", "Cl is the symbol for Chlorine." },
                "Clarence AI", elementerAIPrefab, Camp),
            () => StudentSays("Chlorine.", "So, Sodium and Chlorine..."),

            // ---------------- MCQ 3 (NEW: not in the docx, pairs with PTQ 3) ----------------
            () => ShowMCQ("MCQ 3",
                "What compound do you get when Sodium and Chlorine combine?",
                new string[] { "Sodium Chloride", "Sodium Oxide", "Chlorine Dioxide" },
                "Sodium Chloride",
                new string[] { "Correct!", "Sodium and Chlorine combine to form Sodium Chloride, also known as salt!" },
                new string[] { "Nice try! But unfortunately your answer is wrong!", "Sodium and Chlorine combine to form Sodium Chloride, also known as salt!" },
                "Clarence AI", elementerAIPrefab, Camp),

            // ---------------- ELEMENTER MIX SCENE (make Salt) ----------------
            // same as Mission1 scene 14. MixManager calls nextScene() when the Salt is tapped.
            () =>
            {
                dialogueObject.SetActive(false);
                SceneManager.LoadScene("Elementer", LoadSceneMode.Additive);
                missionCamera.SetActive(false);
                missionEventSystem.SetActive(false);
            },

            // ---------------- AFTER THE MIX ----------------
            () =>
            {
                // same as Mission1 scene 15
                missionCamera.SetActive(true);
                missionEventSystem.SetActive(true);
                _ = SceneManager.UnloadSceneAsync("Elementer");
                StudentSays("It worked!");
            },
            () => TunaSays("Incredible! You really made salt!"),
            () => StudentSays("It's called Sodium Chloride."),
            () => TunaSays("Whatever you call it, you've just saved our meal!"),

            // cutscene placeholder (black text card). If you have a video, copy the video scene from Mission1 (scene 9) here instead.
            () => ShowTextCard("Heneral Tuna adds the salt to the soup and tastes it..."),

            () => TunaSays("Hmm... Perfect!",
                       "Thank you, apprentice. You've done us a great favor.",
                       "If you ever need anything, just say the word. I'll do what I can to help."),

            // ---------------- REPAIR MATERIAL: IRON ----------------
            () => AiSays("*The Elementer suddenly beeps.*", "REPAIR MATERIAL REQUIRED."),
            () => StudentSays("Repair material?"),
            () => AiSays("MATERIAL REQUIRED: IRON"),
            () => TunaSays("Iron? I believe we have some stored in the camp."),
            () => StudentSays("Really?"),
            () => TunaSays("Of course. Come with me."),
            () => TunaSays("*Heneral Tuna hands over a bar of iron.*", "Here. I hope this helps."),
            () => StudentSays("Thanks, Heneral Tuna.", "*You place the iron into the Elementer.*"),
            () => AiSays("IRON DETECTED.", "DATABASE RECALIBRATION INITIATED.", "ASSISTANCE REQUIRED."),

            // ---------------- PTQs (Clarence AI recalibration, same as Mission1) ----------------
            () => ShowPTQ("PTQ 1",
                "Heneral Tuna asks you to add Sodium to the Elementer. Which symbol do you pick?",
                new string[] { "Na", "So", "Sd" },
                "Na",
                "The symbol of Sodium is Na!", Camp),
            () => ShowPTQ("PTQ 2",
                "Salt is made from Sodium and which other element?",
                new string[] { "Chlorine", "Calcium", "Carbon" },
                "Chlorine",
                "Salt is made from Sodium and Chlorine!", Camp),
            () => ShowPTQ("PTQ 3",
                "What is the chemical formula of salt (Sodium Chloride)?",
                new string[] { "NaCl", "ClNa", "NaCl2" },
                "NaCl",
                "The formula of Sodium Chloride is NaCl, one Sodium and one Chlorine!", Camp),

            () => AiSays("DATABASE RECALIBRATION COMPLETE.", "DATABASE UPDATED."),

            // ---------------- END OF CHAPTER ----------------
            // pressing the screen calls nextScene(), which is past the last step, so FinishChapter() runs
            () => ShowTextCard("Chapter 2\n" + chapterTitle + "\nComplete",
                               "\n\nPress the screen to go back to the main menu..."),
        };
    }
}