using UnityEngine;
using TMPro;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.UI;
using System;

public class Dialogue : MonoBehaviour
{
    [Header("Anchors")]
    public Transform characterAnchor;
    public SpriteRenderer backgroundAnchor;

    [Header("Texts")]
    public TextMeshProUGUI textName;
    public TextMeshProUGUI textDialogue;

    [Header("Backgrounds")]
    public Sprite classroomPicture;

    [Header("Dialogue")]
    public float lineSpeed;

    private string[] currentLines;
    private int index;

    public SoundManager soundManager;

    public GameObject loadDialogue(string name, GameObject characterPrefab, string[] lines) { 
        textName.text = name;
        currentLines = lines; // ARRAY TO
        index = 0; // INDEX NUNG LINES

        foreach (Transform child in characterAnchor) Destroy(child.gameObject);
        GameObject currentCharacter = Instantiate(characterPrefab, characterAnchor);

        backgroundAnchor.sprite = classroomPicture;

        showLine();
        return currentCharacter;
    }

    public void showLine() {
        string lineToType = currentLines[index];
        StartCoroutine(typeLine(lineToType));
    }

    public IEnumerator typeLine(string lineToType)
    {
        textDialogue.text = "";
        foreach (char c in lineToType) {
            soundManager.playSoundDialogueLine();

            textDialogue.text = textDialogue.text + c;

            yield return new WaitForSeconds(lineSpeed);
        }
    }

    public void nextLine() {
        soundManager.playSoundNextButton();
        if (textDialogue.text != currentLines[index])
        {
            StopAllCoroutines();
            textDialogue.text = currentLines[index];
        }
        else if (textDialogue.text == currentLines[index] && index < currentLines.Length -1) 
        {
            index++;
            showLine();
        }
        else
        {
            Mission1.Instance.nextScene();
        }
    }
    public void backLine() {
        soundManager.playSoundBackButton();
        if (textDialogue.text != currentLines[index])
        {
            StopAllCoroutines();
            textDialogue.text = currentLines[index];
        }
        else if (textDialogue.text == currentLines[index] && index > 0)
        {
            index--;
            showLine();
        }
        else
        {
            Debug.Log("You're at the end of the line buddy boy");
        }
    }
}
