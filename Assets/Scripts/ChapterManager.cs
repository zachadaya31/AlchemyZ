using JetBrains.Annotations;
using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


[System.Serializable]
public class chapterDetails{
    public int chapterNumber;
    public string chapterTitle;
    public Sprite chapterLogo;
    public bool isLocked;
    
}

public class ChapterManager : MonoBehaviour
{
    [Header("Components ng Chapter Prefab")]
    public GameObject chapterEntryPrefab;
    public Transform content;

    public List<chapterDetails> allChapters;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        loadChapters();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public async void loadChapters() {
        Debug.Log(allChapters[0].chapterTitle);
        
        var student = await SupabaseManager.Instance.supabase
        .From<Students>()
        .Where(s => s.studentID == SessionManager.Instance.studentID)
        .Single();

        int studentChapterProgress = student.studentChapterProgress;

        //unlock yung mga chapters
        for (int a = studentChapterProgress-1; a != (-1); a--)
        {
            allChapters[a].isLocked = false;
        }
        

        for (int z = 0; z < allChapters.Count; z++) {
            GameObject chapterEntry = Instantiate(chapterEntryPrefab, content, false);
            ChapterInformation info = chapterEntry.GetComponentInChildren<ChapterInformation>();

            Button btn = chapterEntry.GetComponent<Button>();

            if (allChapters[z].isLocked == false)
            {
                string chapterNum = "Chapter: " + (z + 1).ToString();
                string chapterTitle = allChapters[z].chapterTitle;
                Sprite newChapterLogo = allChapters[z].chapterLogo;
                info.testDebug(chapterNum, chapterTitle, newChapterLogo);
            }
            else { //KAPAG NAKA LOCK
                btn.enabled = false;
                info.testDebug("", "", null);
            }

            btn.onClick.AddListener(loadScene);
        }
        
        void loadScene() {
            SceneManager.LoadScene("Mission1");
        }
    }
}
