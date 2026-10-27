using UnityEngine;

// Put this on the Clarence AI panel (the object that holds both windows).
// The two tab buttons call ShowChat() and ShowRecipes().
public class ClarenceTabs : MonoBehaviour
{
    public GameObject chatWindow;
    public GameObject recipeWindow;

    // every time the panel opens, start on the chat tab
    void OnEnable()
    {
        ShowChat();
    }

    public void ShowChat()
    {
        chatWindow.SetActive(true);
        recipeWindow.SetActive(false);
    }

    public void ShowRecipes()
    {
        chatWindow.SetActive(false);
        recipeWindow.SetActive(true);
    }
}