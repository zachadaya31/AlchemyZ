using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ChatManager : MonoBehaviour
{
    public TMP_InputField input;     // Input - Chat
    public Transform content;        // Content (inside Viewport)
    public ScrollRect scrollRect;    // Scroll View
    public GameObject studentPrefab; // Chat - Student prefab
    public GameObject profPrefab;    // Chat - Professor prefab

    void Start()
    {
        input.onEndEdit.AddListener(OnSubmit);
    }

    void OnSubmit(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        AddMessage(studentPrefab, text);
        Debug.Log("Ask called: " + text);
        GetComponent<AIConnector>().Ask(text);

        input.text = "";
        input.ActivateInputField(); // keeps the keyboard open for the next message
    }

    // call this for the AI reply too: AddMessage(profPrefab, replyText);
    public void AddMessage(GameObject prefab, string text)
    {
        GameObject row = Instantiate(prefab, content);
        row.GetComponentInChildren<TMP_Text>().text = text;
        StartCoroutine(Refresh(row));
    }

    IEnumerator Refresh(GameObject row)
    {
        yield return null; // wait one frame so the text wraps first
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)row.transform);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)content);
        yield return null;
        scrollRect.verticalNormalizedPosition = 0f;
    }
}