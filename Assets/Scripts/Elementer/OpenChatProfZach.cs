using UnityEngine;

public class OpenChatProfZach : MonoBehaviour
{
    public GameObject chatUIGameObject;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void openChatAI() {
        if (chatUIGameObject != null)
        {
            //if active then hide, if hidden then show
            chatUIGameObject.SetActive(!chatUIGameObject.activeSelf);
        }
    }
}
