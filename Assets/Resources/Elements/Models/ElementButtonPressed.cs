using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ElementButtonPressed : MonoBehaviour
{
    private Button btn;
    private TMP_Text textComp;

    void Awake()
    {
        btn = GetComponent<Button>();
        textComp = GetComponentInChildren<TMP_Text>();

        if (btn != null)
        {
            btn.onClick.AddListener(OnButtonClicked);
        }
    }

    void OnButtonClicked()
    {
        if (textComp != null)
        {
            string rawText = textComp.text;
            string cleanName = rawText.Contains("\n") ? rawText.Split('\n')[1].Trim() : rawText.Trim();

            ElementSelecter.elementSelected = cleanName;
            Debug.Log("Element selected wallahi: " + ElementSelecter.elementSelected);

            if (ElementSelecter.instance != null)
                ElementSelecter.instance.updateElementButtonName(cleanName);
        }

        // closes library + valorant effect, re-enables instructions + warning
        ElementSpawner spawner = FindAnyObjectByType<ElementSpawner>();
        if (spawner != null) spawner.CloseElementLibrary();
    }
}