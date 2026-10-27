using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Android.Gradle;

public class ElementSelecter : MonoBehaviour
{
    public Button buttonElement;
    public static string elementSelected = "Hydrogen";
    public TextMeshProUGUI elementButtonSelected;

    public GameObject valorantEffect;
    public GameObject contentParent;
    //public GameObject elementListRow;
    public GameObject instructions;
    public GameObject warning;
    public static ElementSelecter instance;

    void Awake()
    {
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

        ElementSpawner spawner = FindAnyObjectByType<ElementSpawner>();
        if (spawner != null) spawner.CloseElementLibrary();
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