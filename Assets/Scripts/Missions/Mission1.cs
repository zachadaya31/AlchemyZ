//using Unity.Android.Gradle;
using System;
using System.Collections;
using TMPro;

//using Unity.Android.Gradle;
using Unity.VisualScripting;
//using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Threading.Tasks;

public class Mission1 : MissionBase
{
    public override int ChapterNumber => 1;

    // everything shared (fields, Awake, Start, ShowPTQ, AddScore, video, AR loading) now lives in MissionBase.cs

    public override async void nextScene() {
        IEnumerator waitScene()
        {
            nextButton.interactable = false;
            backButton.interactable = false;
            yield return new WaitForSeconds(1.5f);
            nextButton.interactable = true;
            backButton.interactable = true;
        }
        currentScene++;

        // SCENE 1 -----------------------------------------------------
        // scene 1 is an example ng normal story scene full of dialogues
        if (currentScene == 1)
        {
            StartCoroutine(waitScene());
            fadeAnimator.Play("Fadeout");

            string[] lines = {
                "Okay class... For the last question",
                "What do you get when you combine 2 Hydrogen atoms and 1 Oxygen atom?",
                "How about you, "+ studentFirstName + "?"
            };
            GameObject currentTeacher = dialogueLoader.loadDialogue("Teacher Mikko", teacherPrefab, lines, classroomPicture);
            teacherAnimations = currentTeacher.GetComponent<Animator>();
            if (teacherAnimations != null)
            {
                teacherAnimations.SetTrigger("Speaks");
            }
        }
        // --------------------------------------------------------------




        // SCENE 2 --------------------------------------------------------
        // scene 2 is an example of MCQ scenes where a buncha buttons ay nasa hrap ng screen
        else if (currentScene == 2)
        {

            fadeAnimator.Play("Fadein50");
            backButton.interactable = false;
            nextButton.interactable = false;
            dialogueBox.SetActive(false);

            TextMeshProUGUI question = Instantiate(questionPrefab, buttonsChoicesContainer);
            String questionText = question.text = "What do you get when you combine 2 Hydrogen atoms and 1 Oxygen atom?";

            var recordAnswer = await SupabaseManager.Instance.supabase
            .From<Chapter_Answers_Record>()
            .Where(x => x.studentID == SessionManager.Instance.studentID)
            .Where(x => x.Question == questionText)
            .Get();

            string[] choices = { "Water", "Iron", "Rubber" };
            string correctChoice = "Water";
            for (int i = 0; i < 3; i++)
            {
                GameObject buttonChoices = Instantiate(buttonPrefab, buttonsChoicesContainer);
                buttonChoices.GetComponentInChildren<TMPro.TMP_Text>().text = choices[i];
                string currentChoice = choices[i];
                buttonChoices.GetComponent<Button>().onClick.AddListener(() => { choiceButtonClicked(currentChoice); });
            }

            async void choiceButtonClicked(string choice)
            {
                if (recordAnswer.Models.Count > 0)
                {
                    Debug.Log("student already answered");
                }
                else
                {
                    Debug.Log("no student answer. recording answer");
                    int earnedScore = (choice == correctChoice) ? 1 : 0;
                    var newRecord = new Chapter_Answers_Record
                    {
                        chapterID = 1,
                        MCQ_PCQ = "Chapter 1 | MCQ 1",
                        
                        studentID = SessionManager.Instance.studentID,
                        Question = questionText,
                        Answer = choice,
                        Score = earnedScore
                    };
                    await SupabaseManager.Instance.supabase
                        .From<Chapter_Answers_Record>()
                        .Insert(newRecord);

                    await AddScore(newRecord.MCQ_PCQ, earnedScore); 
                }

                backButton.interactable = true;
                nextButton.interactable = true;
                string[] lines;
                if (choice == "Water")
                {
                    lines = new string[] {
                    "Correct!",
                    "When 2 Hydrogen atoms and 1 Oxygen atom is combined...",
                    "You get... well, Water!",
                    "That's all for today's class. Goodbye Everyone!"
                    };
                }
                else
                {
                    lines = new string[] {
                    "Nice try! But unfortunately your answer is wrong!",
                    "When 2 Hydrogen atoms and 1 Oxygen atom is combined...",
                    "You get... well, Water!",
                    "That's all for today's class. Goodbye Everyone!"
                    };
                }

                foreach (Transform child in buttonsChoicesContainer)
                {
                    Destroy(child.gameObject);
                }
                dialogueBox.SetActive(true);
                fadeAnimator.Play("Fadeout50");
                GameObject currentTeacher = dialogueLoader.loadDialogue("Teacher Mikko", teacherPrefab, lines, classroomPicture);
                teacherAnimations = currentTeacher.GetComponent<Animator>();
                if (teacherAnimations != null)
                {
                    teacherAnimations.SetTrigger("Speaks");
                }
            }
        }
        // --------------------------------------------------------------------------------




        // SCENE 3-------------------------------------------------------------------------
        // scene 3 is an example of "fullscreen button" where a black screen is showed and may text
        // and need mo pindotin to continue... For transition aesthetics lang
        else if (currentScene == 3)
        {
            nextButton.enabled = false;
            backButton.enabled = false;
            fadeAnimator.Play("Fadein");
            StartCoroutine(textFade());

            IEnumerator textFade()
            {
                TextMeshProUGUI question = Instantiate(questionPrefab, buttonsChoicesContainer);
                Animator textAnimations = question.gameObject.GetComponent<Animator>();
                question.text = "As your class ends, Professor Zach texted you to immediately come to the laboratory...";
                textAnimations.Play("TextFadeinNew");

                yield return new WaitForSeconds(3);

                TextMeshProUGUI continueText = Instantiate(questionPrefab, buttonsChoicesContainer);
                Animator textAnimations2 = continueText.gameObject.GetComponent<Animator>();
                continueText.text = "\n\nPress the screen to continue...";
                continueText.fontSize = 50;
                textAnimations2.Play("TextFadeinNew");

                yield return new WaitForSeconds(1);

                GameObject fullscreenButton = Instantiate(fullscreenButtonPrefab, canvasContainer);
                Button btnFs = fullscreenButton.GetComponent<Button>();
                btnFs.onClick.AddListener(() => { nextScene(); Destroy(fullscreenButton); nextButton.enabled = true; backButton.enabled = true; });
            }

        }

        // ---------------------------------------------------------------------------------




        // SCENE 4 ---------------------------------------------------------------------------
        else if (currentScene == 4)
        {
            foreach (Transform child in buttonsChoicesContainer) {
                Destroy(child.gameObject);
            }

            fadeAnimator.Play("Fadeout");

            string[] lines = {
                "My apprentice!",
                "Come! I must show you something!",
                "I've already finished the `Prototype` !!!",
                "Quick, look around and grab it for me!"
            };
            GameObject currentTeacher = dialogueLoader.loadDialogue("Professor Zach", scientistPrefab, lines, laboratoryPicture);
        }

        //--------------------------
        // SCENE 5 - Elementer AR load
        //--------------------------
        // scene 5 is an example sa pag load ng AR sa game
        // AR_Test is just me naming simple pero ang AR_Test ay scene kung saan hahanapin ng student
        // si elementer to pick it up (based on story)

        else if (currentScene == 5) {
            dialogueObject.SetActive(false);
            //SceneManager.LoadScene("AR_Test", LoadSceneMode.Additive);
            StartCoroutine(LoadARScene("AR_Test"));
            missionCamera.SetActive(true);
        }
        

        //--------------------------
        // SCENE 6 - Picked up Elementer
        //--------------------------    

        else if (currentScene == 6) {
            missionCamera.SetActive(true);
            dialogueObject.SetActive(true);

            fadeAnimator.Play("Fadeout");

            string[] lines = {
                "Ah there it is!",
                "Allow me to introduce to you... the Elementer!",
                "This little machine can can combine elements to create anything!",
                "Water, Medicine, anything you can think of!",
                "But first, you must learn its basics.,,",
                "Tell me apprentice, which symbol in the Periodic Table represents Hydrogen?"
            };
            GameObject currentTeacher = dialogueLoader.loadDialogue("Professor Zach", scientistPrefab, lines, laboratoryPicture);
        }

        // mcq scnee
        else if (currentScene == 7)
        {
            fadeAnimator.Play("Fadein50");
            backButton.interactable = false;
            nextButton.interactable = false;
            dialogueBox.SetActive(false);

            
            TextMeshProUGUI question = Instantiate(questionPrefab, buttonsChoicesContainer);
            string questionText = question.text = "Which symbol in the Periodic Table represents Hydrogen?";

            var recordAnswer = await SupabaseManager.Instance.supabase
            .From<Chapter_Answers_Record>()
            .Where(x => x.studentID == SessionManager.Instance.studentID)
            .Where(x => x.Question == questionText)
            .Get();

            string[] choices = { "Hd", "H", "Hy" };
            string correctChoice = "H";
            for (int i = 0; i < 3; i++)
            {
                GameObject buttonChoices = Instantiate(buttonPrefab, buttonsChoicesContainer);
                buttonChoices.GetComponentInChildren<TMPro.TMP_Text>().text = choices[i];
                string currentChoice = choices[i];
                buttonChoices.GetComponent<Button>().onClick.AddListener(() => { choiceButtonClicked(currentChoice); });
            }

            async void choiceButtonClicked(string choice)
            {
                // sa database to ichecheck yung record answer tables or yung history ng answers ng students
                if (recordAnswer.Models.Count > 0)
                {
                    Debug.Log("student already answered");
                }
                else
                {
                    Debug.Log("no student answer. recording answer");
                    int earnedScore = (choice == correctChoice) ? 1 : 0;
                    var newRecord = new Chapter_Answers_Record
                    {
                        chapterID = 1,
                        MCQ_PCQ = "Chapter 1 | MCQ 2",
                        
                        studentID = SessionManager.Instance.studentID,
                        Question = questionText,
                        Answer = choice,
                        Score = earnedScore
                    };
                    await SupabaseManager.Instance.supabase
                        .From<Chapter_Answers_Record>()
                        .Insert(newRecord);

                        await AddScore(newRecord.MCQ_PCQ, earnedScore); 
                }

                backButton.interactable = true;
                nextButton.interactable = true;
                string[] lines;
                if (choice == "H")
                {
                    lines = new string[] {
                        "Correct!",
                        "The symbol that represents Hydrogen is H!",
                        "Now, "+studentFirstName + ", my apprentice. Try out the Elementer!",
                    };
                }
                else
                {
                    lines = new string[] {
                        "Nope, your answer is wrong!",
                        "The symbol that represents Hydrogen is H!",
                        "Now, "+studentFirstName + ", my apprentice. Try out the Elementer!",
                    };
                }

                foreach (Transform child in buttonsChoicesContainer)
                {
                    Destroy(child.gameObject);
                }
                dialogueBox.SetActive(true);
                fadeAnimator.Play("Fadeout50");
                GameObject currentTeacher = dialogueLoader.loadDialogue("Professor Zach", scientistPrefab, lines, laboratoryPicture);
                teacherAnimations = currentTeacher.GetComponent<Animator>();
                if (teacherAnimations != null)
                {
                    teacherAnimations.SetTrigger("Speaks");
                }
            }
        }

        //--------------------------
        // SCENE 8 - Elementer AR Tutorial Scene
        //--------------------------

        else if (currentScene == 8) {
            fadeAnimator.Play("Fadeout");
            dialogueObject.SetActive(false);
            StartCoroutine(LoadARScene("Elementer_Tutorial"));
            missionCamera.SetActive(true);
        }

        //--------------------------
        // Scene 9
        //--------------------------
        //scene 9 is an example of scenes kung saan may video na nag pplay to show the story progressing

        else if (currentScene == 9)
        {
            dialogueObject.SetActive(false);
            backButton.interactable = false;
            nextButton.interactable = false;

            videoPlayerObject.SetActive(true);
            videoPlayer.Play();
            videoPlayer.loopPointReached += OnVideoFinished;
        }

        //--------------------------
        // SCENE 9 - NASA FOREST NA YUNG STUDENT
        //--------------------------
        else if (currentScene == 10)
        {
                
                videoPlayerObject.SetActive(false);
                foreach (Transform child in buttonsChoicesContainer) {
                    Destroy(child.gameObject);
                }

                fadeAnimator.Play("Fadeout");

                string[] lines = {
                    "Where am I?",
                    "...",
                    "I think i'm gonna pass out...",
                    "I need some water..."
                };
                GameObject currentTeacher = dialogueLoader.loadDialogue(studentFirstName, emptyPrefab, lines, forestBackground);
                backgroundAnchor.transform.position = new Vector3(-4.28f, -0.65f, -1.75f);
                
        }

        else if (currentScene == 11)
        {
            dialogueObject.SetActive(false);
            backButton.interactable = false;
            nextButton.interactable = false;


            videoPlayerObject2.SetActive(true);
            videoPlayer2.Play();
            videoPlayer2.loopPointReached += OnVideoFinished;
        }

        else if (currentScene == 12)
        {
                
                videoPlayerObject.SetActive(false);
                foreach (Transform child in buttonsChoicesContainer) {
                    Destroy(child.gameObject);
                }

                fadeAnimator.Play("Fadeout");

                string[] lines = {
                    "...",
                    "Is this the Elementer..?",
                    "I'll try to make some water to drink.",
                    "Ok before that, do I remember how to make water?",
                    "H2O... meaning there are how many Hydrogens in it?"
                };
                GameObject currentTeacher = dialogueLoader.loadDialogue(studentFirstName, elementerPrefab, lines, forestBackground);
                backgroundAnchor.transform.position = new Vector3(-4.28f, -0.65f, -1.75f);
                
        }

        else if (currentScene == 13)
        {
            fadeAnimator.Play("Fadein50");
            backButton.interactable = false;
            nextButton.interactable = false;
            dialogueBox.SetActive(false);

            
            TextMeshProUGUI question = Instantiate(questionPrefab, buttonsChoicesContainer);
            string questionText = question.text = "How many Hydrogen atoms are required to create water?";

            var recordAnswer = await SupabaseManager.Instance.supabase
            .From<Chapter_Answers_Record>()
            .Where(x => x.studentID == SessionManager.Instance.studentID)
            .Where(x => x.Question == questionText)
            .Get();

            string[] choices = { "1", "2", "3" };
            string correctChoice = "2";
            for (int i = 0; i < 3; i++)
            {
                GameObject buttonChoices = Instantiate(buttonPrefab, buttonsChoicesContainer);
                buttonChoices.GetComponentInChildren<TMPro.TMP_Text>().text = choices[i];
                string currentChoice = choices[i];
                buttonChoices.GetComponent<Button>().onClick.AddListener(() => { choiceButtonClicked(currentChoice); });
            }

            async void choiceButtonClicked(string choice)
            {
                // sa database to ichecheck yung record answer tables or yung history ng answers ng students
                if (recordAnswer.Models.Count > 0)
                {
                    Debug.Log("student already answered");
                }
                else
                {
                    Debug.Log("no student answer. recording answer");
                    int earnedScore = (choice == correctChoice) ? 1 : 0;
                    var newRecord = new Chapter_Answers_Record
                    {
                        chapterID = 1,
                        MCQ_PCQ = "Chapter 1 | MCQ 3",
                        
                        studentID = SessionManager.Instance.studentID,
                        Question = questionText,
                        Answer = choice,
                        Score = earnedScore
                    };
                    await SupabaseManager.Instance.supabase
                        .From<Chapter_Answers_Record>()
                        .Insert(newRecord);

                        await AddScore(newRecord.MCQ_PCQ, earnedScore); 
                }

                backButton.interactable = true;
                nextButton.interactable = true;
                string[] lines;
                if (choice == "2")
                {
                    lines = new string[] {
                        "I think this is it.",
                        "2 Hydrogen atoms are needed in the formula H2O.",
                        "Alright, let's give this a shot. Let's try creating water.",
                    };
                }
                else
                {
                    lines = new string[] {
                        "Hmmm, I think 2 Hydrogen atoms are needed since the formula states H2 inside H2O",
                        "Yeah, I think that's quite right. 2 Hydrogen atoms.",
                        "Alright, let's give this a shot. Let's try creating water.",
                    };
                }

                foreach (Transform child in buttonsChoicesContainer)
                {
                    Destroy(child.gameObject);
                }
                dialogueBox.SetActive(true);
                fadeAnimator.Play("Fadeout50");
                GameObject currentTeacher = dialogueLoader.loadDialogue(studentFirstName, emptyPrefab, lines, forestBackground);
                teacherAnimations = currentTeacher.GetComponent<Animator>();
                if (teacherAnimations != null)
                {
                    teacherAnimations.SetTrigger("Speaks");
                }
            }
        }


        else if (currentScene == 14) {

            dialogueObject.SetActive(false);
            SceneManager.LoadScene("Elementer", LoadSceneMode.Additive);
            missionCamera.SetActive(false);
            missionEventSystem.SetActive(false);

        }

        else if (currentScene == 15)
        {      
            missionCamera.SetActive(true);
            missionEventSystem.SetActive(true);

            _ = SceneManager.UnloadSceneAsync("Elementer");

            dialogueObject.SetActive(true);
            videoPlayerObject.SetActive(false);
            foreach (Transform child in buttonsChoicesContainer)
            {
                Destroy(child.gameObject);
            }

            fadeAnimator.Play("Fadeout");

            string[] lines = {
                    "It worked!",
                    "I just made water using the Elementer!!!",
                    "Ok... I have a lot of questions as of now but I think it's best if I hit the hay for tonight.",
                };
            GameObject currentTeacher = dialogueLoader.loadDialogue(studentFirstName, elementerPrefab, lines, forestBackground);
            backgroundAnchor.transform.position = new Vector3(-4.28f, -0.65f, -1.75f);

        }

        // another black screen with full screen button
        else if (currentScene == 16)
        {
            dialogueObject.SetActive(false);
            nextButton.enabled = false;
            backButton.enabled = false;

            missionCamera.SetActive(true);
            missionEventSystem.SetActive(true);

            fadeAnimator.Play("Fadein");
            StartCoroutine(textFade());

            IEnumerator textFade()
            {
                TextMeshProUGUI question = Instantiate(questionPrefab, buttonsChoicesContainer);
                Animator textAnimations = question.gameObject.GetComponent<Animator>();
                question.text = "Hours pass as you went asleep...";
                textAnimations.Play("TextFadeinNew");

                yield return new WaitForSeconds(3);

                TextMeshProUGUI continueText = Instantiate(questionPrefab, buttonsChoicesContainer);
                Animator textAnimations2 = continueText.gameObject.GetComponent<Animator>();
                continueText.text = "\n\nPress the screen to continue...";
                continueText.fontSize = 50;
                textAnimations2.Play("TextFadeinNew");

                yield return new WaitForSeconds(1);

                GameObject fullscreenButton = Instantiate(fullscreenButtonPrefab, canvasContainer);
                Button btnFs = fullscreenButton.GetComponent<Button>();
                btnFs.onClick.AddListener(() => { nextScene(); Destroy(fullscreenButton); nextButton.enabled = true; backButton.enabled = true; });
            }

        }


        else if (currentScene == 17)
        {
            dialogueObject.SetActive(true);
            videoPlayerObject.SetActive(false);
            foreach (Transform child in buttonsChoicesContainer)
            {
                Destroy(child.gameObject);
            }

            fadeAnimator.Play("Fadeout");

            string[] lines = {
                    "..?",
                    "Huh?",
                };
            GameObject currentTeacher = dialogueLoader.loadDialogue(studentFirstName, emptyPrefab, lines, forestNightBackground);
            backgroundAnchor.transform.position = new Vector3(-4.28f, -0.65f, -1.75f);

        }

        // forst night scene
        else if (currentScene == 18)
        {
            foreach (Transform child in buttonsChoicesContainer) {
                Destroy(child.gameObject);
            }
            fadeAnimator.Play("Fadeout");

            dialogueObject.SetActive(false);
            backButton.interactable = false;
            nextButton.interactable = false;

            videoPlayerObject3.SetActive(true);
            videoPlayer3.isLooping = false;
            videoPlayer3.prepareCompleted += (vp) => vp.Play();
            videoPlayer3.loopPointReached += OnVideoFinished;
            videoPlayer3.Prepare();
        }

        else if (currentScene == 19)
        {
            dialogueObject.SetActive(true);
            videoPlayerObject.SetActive(false);
            foreach (Transform child in buttonsChoicesContainer)
            {
                Destroy(child.gameObject);
            }

            fadeAnimator.Play("Fadeout");

            string[] lines = {
                "Professor Zach?!",
                "Is that you?!",
            };

            GameObject currentTeacher = dialogueLoader.loadDialogue(studentFirstName, emptyPrefab, lines, forestNightBackground);
            
            backgroundAnchor.transform.position = new Vector3(-4.28f, -0.65f, -1.75f);
        }

        // an example of clarence AI scene kung saan nakikipag usap si student kay elementer AI (clarence)
        // used for future references kung kakausapin uli ni student si AI
        else if (currentScene == 20)
        {
            dialogueObject.SetActive(true);
            videoPlayerObject.SetActive(false);
            foreach (Transform child in buttonsChoicesContainer)
            {
                Destroy(child.gameObject);
            }

            fadeAnimator.Play("Fadeout");

            string[] lines = {
                studentFirstName + ", my apprentice!",
                "I'm not exactly Professor Zach...",
                "I'm an AI named Clarence built into the Elementer by Prof. Zach!!!",
                "It seems that the Elementer overloaded and it accidentally opened up an interdimensional portal!",
                "Based on my calculations, you and Professor Zach was put into different dimensions.",
            };

            GameObject currentTeacher = dialogueLoader.loadDialogue("Clarence AI", elementerAIPrefab, lines, forestNightBackground);
            
            backgroundAnchor.transform.position = new Vector3(-4.28f, -0.65f, -1.75f);
        }

        else if (currentScene == 21)
        {
            dialogueObject.SetActive(true);
            videoPlayerObject.SetActive(false);
            foreach (Transform child in buttonsChoicesContainer)
            {
                Destroy(child.gameObject);
            }

            fadeAnimator.Play("Fadeout");

            string[] lines =
            {
                "WHAT?!",
                "Another dimension?!",
                "What do you mean?!"
            };

            GameObject currentTeacher = dialogueLoader.loadDialogue(studentFirstName, emptyPrefab, lines, forestNightBackground);
            
            backgroundAnchor.transform.position = new Vector3(-4.28f, -0.65f, -1.75f);
        }

        else if (currentScene == 22)
        {
            dialogueObject.SetActive(true);
            videoPlayerObject.SetActive(false);
            foreach (Transform child in buttonsChoicesContainer)
            {
                Destroy(child.gameObject);
            }

            fadeAnimator.Play("Fadeout");

            string[] lines =
            {
                "I know this is a lot to uncover and this might overwhelm you.",
                "This is eactly why Professor Zach built an AI like me!",
                "Don't worry I'll get us out of this.",
                "But first, because of the overloading, my database seems to be miscalibrated...",
                "I need you to answer these questions just to refresh the Elementer's Information Databse",
                "Let's start."
            };

            GameObject currentTeacher = dialogueLoader.loadDialogue("Clarence AI", elementerAIPrefab, lines, forestNightBackground);
            
            backgroundAnchor.transform.position = new Vector3(-4.28f, -0.65f, -1.75f);
        }
        
        
        // MGA PTQ
        else if (currentScene == 23)
        {
            ShowPTQ("PTQ 1",
                "Which combination of atoms makes one molecule of water?",
                new string[] { "1 Hydrogen and 2 Oxygen", "2 Hydrogen and 1 Oxygen", "2 Hydrogen and 2 Oxygen" },
                "2 Hydrogen and 1 Oxygen",
                "Water is made from 2 Hydrogen atoms and 1 Oxygen atom!");
        }
        else if (currentScene == 24)
        {
            ShowPTQ("PTQ 2",
                "Which element does the symbol \"H\" stand for on the Periodic Table?",
                new string[] { "Helium", "Hafnium", "Hydrogen" },
                "Hydrogen",
                "The symbol H stands for Hydrogen!");
        }
        else if (currentScene == 25)
        {
            ShowPTQ("PTQ 3",
                "In the formula H2O, what does the small \"2\" tell you?",
                new string[] { "There are 2 Oxygen atoms", "There are 2 Hydrogen atoms", "There are 2 water molecules" },
                "There are 2 Hydrogen atoms",
                "The small 2 after the H means there are 2 Hydrogen atoms in one water molecule!");
        }

        // end scene
        else if (currentScene == 26)
        {
            dialogueObject.SetActive(false);
            nextButton.enabled = false;
            backButton.enabled = false;

            missionCamera.SetActive(true);
            missionEventSystem.SetActive(true);

            fadeAnimator.Play("Fadein");
            StartCoroutine(textFade());

            IEnumerator textFade()
            {
                TextMeshProUGUI question = Instantiate(questionPrefab, buttonsChoicesContainer);
                Animator textAnimations = question.gameObject.GetComponent<Animator>();
                    question.text = "Chapter 1\nThe Alchemist's Apprentice\nComplete";
                    textAnimations.Play("TextFadeinNew");

                    yield return new WaitForSeconds(3);

                    TextMeshProUGUI continueText = Instantiate(questionPrefab, buttonsChoicesContainer);
                Animator textAnimations2 = continueText.gameObject.GetComponent<Animator>();
                continueText.text = "\n\nPress the screen to go back to the main menu...";
                continueText.fontSize = 50;
                textAnimations2.Play("TextFadeinNew");

                yield return new WaitForSeconds(1);

                GameObject fullscreenButton = Instantiate(fullscreenButtonPrefab, canvasContainer);
                Button btnFs = fullscreenButton.GetComponent<Button>();
                btnFs.onClick.AddListener(() => { nextScene(); Destroy(fullscreenButton); nextButton.enabled = true; backButton.enabled = true; });
            }

        }

        else
        {
            Debug.Log("End of mission 1");
            FinishChapter(); // unlocks chapter 2, then main menu

        }

        
    }
}