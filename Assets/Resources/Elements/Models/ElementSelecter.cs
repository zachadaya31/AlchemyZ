using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ElementSelecter : MonoBehaviour
{
    public Button buttonElement;
    public static string elementSelected = "Hydrogen";
    public TextMeshProUGUI elementButtonSelected;

    public GameObject valorantEffect;
    public GameObject contentParent;
    public static ElementSelecter instance;

    void Awake()
    {
        // only the copy with the text slot filled becomes the instance
        if (elementButtonSelected != null)
        {
            instance = this;
        }
    }

    public void SelectElement()
    {
        TMP_Text buttonTextComponent = buttonElement.GetComponentInChildren<TMP_Text>();

        if (buttonTextComponent != null)
        {
            elementSelected = buttonTextComponent.text;
            Debug.Log("Button text is: " + elementSelected);
            updateElementButtonName(elementSelected);
        }
        valorantEffect.SetActive(false);
        contentParent.SetActive(false);
    }

    void Start()
    {
        updateElementButtonName(elementSelected);
    }

    public void updateElementButtonName(string elementName)
    {
        if (elementButtonSelected == null) return;
        elementButtonSelected.SetText("Element:" + "\n" + elementName);
    }
}