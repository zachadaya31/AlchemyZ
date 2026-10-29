using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Put on the eye Button. Make one eye button per password field.
[RequireComponent(typeof(Button))]
public class PasswordToggle : MonoBehaviour
{
    public TMP_InputField inputField; 
    public Image eyeImage;
    public Sprite eyeOpen;  
    public Sprite eyeClosed;

    private bool visible;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(Toggle);
        Apply();
    }

    public void Toggle()
    {
        visible = !visible;
        Apply();
    }

    private void Apply()
    {
        inputField.contentType = visible ? TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.Password;
        inputField.ForceLabelUpdate();

        if (eyeImage != null)
        {
            if (visible && eyeOpen != null) eyeImage.sprite = eyeOpen;
            else if (!visible && eyeClosed != null) eyeImage.sprite = eyeClosed;
        }
    }
}