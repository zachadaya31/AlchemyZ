using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChapterInformation : MonoBehaviour
{
    public TextMeshProUGUI chapterNumTxt;
    public TextMeshProUGUI chapterTitleTxt;
    public Image chapterLogoSprite;
    public Image chapterBackground;
    public Image chapterLocked;

    public void testDebug(string chapterNum, string chapterTitle, Sprite chapterLogo) {
        chapterNumTxt.SetText(chapterNum);
        chapterTitleTxt.SetText(chapterTitle);

        if (chapterLogo == null)
        {
            chapterLogoSprite.enabled = false;
            chapterBackground.sprite = chapterLocked.sprite;
        }
        else {
            chapterLogoSprite.sprite = chapterLogo;
        }
    }
}
