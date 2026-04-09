using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


[System.Serializable]
public class chapterDetails{
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

    public void loadChapters() {
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
            SceneManager.LoadScene("Story");
        }
    }
}
