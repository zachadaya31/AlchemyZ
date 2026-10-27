using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ChatManager : MonoBehaviour
{
    public TMP_InputField input;
    public Transform content;
    public ScrollRect scrollRect;  
    public GameObject studentPrefab; 
    public GameObject profPrefab;

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

        ShowTyping();

        input.text = "";
        input.ActivateInputField();
    }

    public GameObject AddMessage(GameObject prefab, string text)
    {
        GameObject row = Instantiate(prefab, content);
        row.GetComponentInChildren<TMP_Text>().text = text;
        StartCoroutine(Refresh(row));
        return row;
    }

    IEnumerator Refresh(GameObject row)
    {
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)row.transform);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)content);
        yield return null;
        scrollRect.verticalNormalizedPosition = 0f;
    }

    GameObject typingRow;
    Coroutine typingRoutine;

    public void ShowTyping()
    {
        typingRow = AddMessage(profPrefab, ".");
        typingRoutine = StartCoroutine(Dots(typingRow.GetComponentInChildren<TMP_Text>()));
    }

    public void HideTyping()
    {
        if (typingRoutine != null) StopCoroutine(typingRoutine);
        if (typingRow != null) Destroy(typingRow);
        typingRow = null;
        typingRoutine = null;
    }

    IEnumerator Dots(TMP_Text label)
    {
        string[] frames = { ".", "..", "..." };
        int i = 0;
        while (true)
        {
            label.text = frames[i];
            i = (i + 1) % frames.Length;
            yield return new WaitForSeconds(0.4f);
        }
    }
}