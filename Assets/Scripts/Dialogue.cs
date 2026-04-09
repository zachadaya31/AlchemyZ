using UnityEngine;
using TMPro;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.UI;
using System;

public class Dialogue : MonoBehaviour
{
    public Transform characterAnchor;
    public TextMeshProUGUI textName;
    public TextMeshProUGUI textComponent;
    public Renderer backgroundImage;
    public string[] lines;
    public float textSpeed;

    private int index;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        textComponent.text = string.Empty;
        startDialogue();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void spawnCharacter(GameObject characterPrefab) {
        foreach (Transform child in characterAnchor) {
            Destroy(child.gameObject);
        }
        GameObject newActor = Instantiate(characterPrefab, characterAnchor);
        newActor.transform.localPosition = Vector3.zero;
    }

    void startDialogue() { 
        index = 0;
        StartCoroutine(TypeLines());
    }

    IEnumerator TypeLines() { 
        foreach(char c in lines[index].ToCharArray())
        {
            textComponent.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
    }

    void playLine(int mikko) {
        // new index is either 0 + 1 or 0 - 1, or mikko int
        int newIndex = index + mikko;

        if (newIndex >= 0 && newIndex < lines.Length) {
            index = newIndex;
            textComponent.text = string.Empty;
            StartCoroutine(TypeLines());
        }
        else if (newIndex == -1){
            Debug.Log(index);
            Debug.Log(newIndex);
            Debug.Log(lines.Length);
            // Index = 0
            // newIndex = -1
            // Lines = 3
            // yung lines ay kung ilang dialouge lines and meron
            // yung new index is where the index wants to go next,
            // so if -1 yung gusto puntahan, di tayo papayag
        }
        else {
            gameObject.SetActive(false);
        }

    }

    public void nextButtonPressed() {
        if (textComponent.text == lines[index]) {
            playLine(1);
        }
        else {
            StopAllCoroutines();
            textComponent.text = lines[index];
        }
    }

    public void backButtonPressed() {
        if (textComponent.text == lines[index]) {
            playLine(-1);
        }
        else {
            StopAllCoroutines();
            textComponent.text = lines[index];
        }
    }

    public void loadConversation(string characterPrefabName, GameObject characterPrefab, string[] newLines)
    {
        textName.text = characterPrefabName;
        lines = newLines;
        spawnCharacter(characterPrefab);
        StopAllCoroutines();
        textComponent.text = string.Empty;
        startDialogue();
    }
}
