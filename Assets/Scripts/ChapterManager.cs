using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// NOTE: the old ChapterRow class was removed. The chapters model is now ChapterDbRow in SupabaseManager.cs

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

    void Start()
    {
        loadChapters();
    }

    public async void loadChapters() {

        // 1. the chapters that exist (from the database), sorted 1 to 10 by chapterNumber
        List<ChapterDbRow> chapters = await SupabaseManager.Instance.GetChapters();

        // 2. how far this student got (the database knows who is logged in)
        int studentChapterProgress = await SupabaseManager.Instance.GetUnlockedChapter();

        // 3. one entry per chapter in the database
        foreach (ChapterDbRow chapter in chapters)
        {
            int number = chapter.chapterNumber;
            bool unlocked = number <= studentChapterProgress;

            GameObject chapterEntry = Instantiate(chapterEntryPrefab, content, false);
            ChapterInformation info = chapterEntry.GetComponentInChildren<ChapterInformation>();
            Button btn = chapterEntry.GetComponent<Button>();

            // logo comes from the Inspector list (by chapter number). If the title is empty in the database, use the Inspector one
            string title = chapter.title;
            Sprite logo = null;
            int listIndex = number - 1;
            if (listIndex >= 0 && listIndex < allChapters.Count)
            {
                logo = allChapters[listIndex].chapterLogo;
                if (string.IsNullOrEmpty(title)) title = allChapters[listIndex].chapterTitle;
            }

            if (unlocked)
            {
                info.testDebug("Chapter: " + number, title, logo);
            }
            else { //KAPAG NAKA LOCK
                btn.enabled = false;
                info.testDebug("", "", null);
            }

            btn.onClick.AddListener(() => openChapter(number));
        }
    }

    void openChapter(int number)
    {
        string sceneName = "Mission" + number;

        // para sa error ng chapter pag wala
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.Log(sceneName + " is not in Build Settings yet (coming soon).");
            return;
        }

        SceneLoader.loadScene(sceneName);
    }
}