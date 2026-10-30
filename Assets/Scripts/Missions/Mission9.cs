using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Mission9 : MissionBase
{
    public override int ChapterNumber => 9;

    public override string RequiredMixResult => "Calcium Chloride";
    public override string MixInstructions => "Make the salt that can break the ice.\nNot sure how? Ask Clarence AI for a hint.";

    private const string chapterTitle = "The Frozen Harbor";

    [Header("Chapter 9 only")]
    public GameObject ilsaPrefab;          // Harbor Master Ilsa
    public Sprite harborBackground;      // frozen harbor at dawn (empty = forest night)

    private List<Action> steps;

    private Sprite Bg { get { return harborBackground != null ? harborBackground : forestNightBackground; } }

    private void StudentSays(params string[] lines) { Say(studentFirstName, emptyPrefab, Bg, lines); }
    private void IlsaSays(params string[] lines) { Say("Harbor Master Ilsa", ilsaPrefab, Bg, lines); }
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
            Debug.Log("End of mission 9");
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
            () => { StartCoroutine(WaitScene()); StudentSays("Whoa. Everything is frozen solid."); },
            () => AiSays("Your readings are steady. Scanning the area now.",
                         "Still no sign of the professor, but the trace is stronger here."),
            () => StudentSays("Excuse me. What happened here?"),
            () => IlsaSays("Who are you? You're not off any ship of mine."),
            () => StudentSays("I'm a traveler. Lost, honestly."),
            () => IlsaSays("Lost in this weather. Forgive me if I make a poor host.",
                           "The harbor froze overnight. Every ship is locked in.",
                           "The grain ship must sail before the storm, and my axes barely scratch the ice."),
            () => StudentSays("Can't you light fires along the dock?"),
            () => IlsaSays("We would burn the planks before the ice felt a thing.",
                           "My uncle had a white salt. He scattered it on ice and the ice cracked and wept. The salt even grew warm in his hands.",
                           "He never told me how it was made. Only that it came from a grey metal and a pale green gas."),
            () => AiSays("PARTIAL MATCH FOUND."),
            () => StudentSays("A grey metal, a green gas, and a salt that warms up and melts ice... That's calcium chloride.",
                              "I think I can make it. With this.", "*You hold up the Elementer.*"),
            () => IlsaSays("What strange thing is that?"),
            () => StudentSays("A machine that combines elements. Let me try.", "*You open the Elementer.*"),

            // ---------------- THE STORY TEACHES: heat, then the six phase changes ----------------
            () => AiSays("Here is the equation: Ca + Cl2 -> CaCl2.",
                         "Two substances join into one. That's a combination reaction, and it's balanced."),
            () => StudentSays("But how does a salt melt ice?"),
            () => AiSays("Heat is energy. When a substance gets heat, its particles move faster. When it loses heat, they slow down.",
                         "Calcium chloride gives off heat when it dissolves in water.",
                         "That heat wakes up the ice particles, and they move faster until the ice melts."),
            () => StudentSays("Warm things have fast particles. Cold things have slow ones."),
            () => AiSays("Right. Now the six phase changes. Three of them need energy.",
                         "Melting: solid to liquid, like ice turning to water.",
                         "Vaporization: liquid to gas, like a puddle drying up.",
                         "Sublimation: solid straight to gas, with no liquid in between."),
            () => StudentSays("Like the mothballs in my grandma's closet. They shrink but never get wet."),
            () => AiSays("Exactly. The other three give out energy, and they're the reverse.",
                         "Freezing: liquid to solid.",
                         "Condensation: gas to liquid, like fog on a cold mirror.",
                         "Deposition: gas straight to solid, with no liquid in between."),
            () => StudentSays("So sublimation and deposition both skip the liquid.", "One goes up, the other goes down."),
            () => IlsaSays("I follow little of this. Start simple.", "The salt warms the ice. What does heat do to the tiny pieces inside it?"),

            // ---------------- MCQs ----------------
            () => ShowMCQ("Chapter "+MissionBase.Instance.ChapterNumber +"| MCQ 1",
                "What happens to the particles of a substance when it is heated?",
                new string[] { "They slow down", "They move faster", "They get bigger" },
                "They move faster",
                new string[] { "Correct!", "Heat gives particles energy, so they move faster." },
                new string[] { "Nice try! But unfortunately your answer is wrong!", "Heat gives particles energy, so they move faster." },
                "Clarence AI", elementerAIPrefab, Bg),
            () => StudentSays("They gain energy and move faster. That's how ice melts."),
            () => IlsaSays("Then here is another puzzle.", "The snow on my rooftops shrinks in this dry wind, yet not one drop of water runs off."),

            () => ShowMCQ("Chapter "+MissionBase.Instance.ChapterNumber +"| MCQ 2",
                "What is it called when a solid turns directly into a gas without becoming a liquid?",
                new string[] { "Sublimation", "Condensation", "Melting" },
                "Sublimation",
                new string[] { "Correct!", "Sublimation is solid straight to gas, with no liquid in between." },
                new string[] { "Nice try! But unfortunately your answer is wrong!", "Sublimation is solid straight to gas, with no liquid in between." },
                "Clarence AI", elementerAIPrefab, Bg),
            () => StudentSays("Sublimation. The snow goes from solid straight to gas, so there's no water."),
            () => IlsaSays("Remarkable. And look at my ropes. White fur on them every dawn, though no rain fell."),
            () => StudentSays("That frost came from water vapor in the air."),
            () => IlsaSays("And what is that change called?"),

            () => ShowMCQ("Chapter "+MissionBase.Instance.ChapterNumber +"| MCQ 3",
                "What is it called when a gas turns directly into a solid?",
                new string[] { "Freezing", "Sublimation", "Deposition" },
                "Deposition",
                new string[] { "Correct!", "Deposition is gas straight to solid, with no liquid in between." },
                new string[] { "Nice try! But unfortunately your answer is wrong!", "Deposition is gas straight to solid, with no liquid in between." },
                "Clarence AI", elementerAIPrefab, Bg),
            () => StudentSays("Deposition. Gas straight to solid."),
            () => IlsaSays("Good. Now make your salt. The storm will not wait."),

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
                StudentSays("It worked! White crystals are pouring into the sack!");
            },

            () => IlsaSays("Gloves on, everyone! My uncle said it burns bare skin when wet."),

            () => ShowTextCard("The workers scatter the crystals along the hull. The ice cracks, hisses, and turns to water around the ship. The grain ship slips out of the harbor..."),

            () => IlsaSays("Free! Every ship, free.",
                           "You saved us from the storm, and I don't even know your name.",
                           "Please, take this nickel compass case. A sailor left it behind years ago. It's a small thanks."),

            // ---------------- REPAIR MATERIAL: NICKEL ----------------
            () => StudentSays("Nickel... the Elementer is glowing again."),
            () => AiSays("REPAIR MATERIAL REQUIRED: NICKEL."),
            () => StudentSays("*You place the nickel into the Elementer.*"),
            () => AiSays("NICKEL DETECTED.", "DATABASE RECALIBRATION STARTED.", "ASSISTANCE REQUIRED."),

            // ---------------- PTQs ----------------
            () => ShowPTQ("Chapter "+MissionBase.Instance.ChapterNumber +"| PTQ 1",
                "Ice at -10 degrees C is warmed to -2 degrees C. What happens to its particles?",
                new string[] { "They vibrate faster", "They vibrate slower", "They disappear" },
                "They vibrate faster",
                "Heat gives particles more energy, so they vibrate faster!", Bg),
            () => ShowPTQ("Chapter "+MissionBase.Instance.ChapterNumber +"| PTQ 2",
                "Mothballs in a closet get smaller over time without ever turning into a liquid. Which change is this?",
                new string[] { "Melting", "Freezing", "Sublimation" },
                "Sublimation",
                "The solid turns straight into a gas, so it is sublimation!", Bg),
            () => ShowPTQ("Chapter "+MissionBase.Instance.ChapterNumber +"| PTQ 3",
                "Iodine vapor touches a cold glass plate and forms purple crystals without becoming a liquid first. Which change is this?",
                new string[] { "Condensation", "Sublimation", "Deposition" },
                "Deposition",
                "The gas turns straight into a solid, so it is deposition!", Bg),

            // ---------------- RECALIBRATION DONE + ELEMENTS UNLOCKED ----------------
            () => AiSays("DATABASE RECALIBRATION COMPLETE.", "PHASE CHANGE FILES RESTORED.", ElementLocks.Unlock(9)),

            // ---------------- CHAPTER END ----------------
            () => IlsaSays("Whoever you're looking for, I hope the sea brings you to them."),
            () => AiSays(studentFirstName + ", the nickel cleared almost all the noise.",
                         "Signal strength: 90%. Now I can finally read it clearly."),
            () => StudentSays("Is he here?"),
            () => AiSays("No. He isn't in this world. But we're much closer than before.",
                         "The trace has stopped drifting. It's locked on one spot."),
            () => StudentSays("Where?"),
            () => AiSays("Somewhere hot. Not cold, like I thought. The noise was hiding it.",
                         "I'm sure now. The next jump is where the professor is.",
                         "NEXT TRACE DETECTED. PORTAL CHARGING."),
            () => StudentSays("This is it. We're bringing you home, Professor.", "*The Elementer glows, ready for the next jump.*"),

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

            () => ShowTextCard("Chapter 9\n" + chapterTitle + "\nComplete",
                               "\n\nPress the screen to go back to the main menu..."),
        };
    }
}
