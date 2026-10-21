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
        // 1. Read its own text string and clean it up (e.g., "1\nHydrogen" -> "Hydrogen")
        if (textComp != null)
        {
            string rawText = textComp.text;
            string cleanName = rawText.Contains("\n") ? rawText.Split('\n')[1].Trim() : rawText.Trim();

            // Set the static global variable
            ElementSelecter.elementSelected = cleanName;
            Debug.Log("Element selected wallahi: " + ElementSelecter.elementSelected);
        }

        // 2. Find and close Valorant Effect in the scene
        GameObject valorantEffect = GameObject.Find("Valorant Effect");
        if (valorantEffect != null)
        {
            valorantEffect.SetActive(false);
        }

        // 3. Find and close Element Library in the scene
        GameObject elementLibrary = GameObject.Find("Element Library");
        if (elementLibrary != null)
        {
            elementLibrary.SetActive(false);
        }
    }
}