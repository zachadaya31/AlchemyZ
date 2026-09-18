using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ElementSelecter : MonoBehaviour
{
    public Button buttonElement;
    public static string elementSelected = "";

    public GameObject valorantEffect;
    public GameObject contentParent;

    public void SelectElement() {
        TMP_Text buttonTextComponent = buttonElement.GetComponentInChildren<TMP_Text>();

        if (buttonTextComponent != null)
        {
            string buttonText = buttonTextComponent.text;
            ElementSelecter.elementSelected = buttonText;
            Debug.Log("Button text is: " + buttonText);
        }
        valorantEffect.SetActive(false);
        contentParent.SetActive(false);

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
