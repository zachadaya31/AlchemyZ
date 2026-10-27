using UnityEngine;
using TMPro;

public class ElementSearch : MonoBehaviour
{
    public TMP_InputField searchInput;
    public Transform content;

    void Start()
    {
        searchInput.onValueChanged.AddListener(Filter);
    }

    void Filter(string search)
    {
        search = search.Trim().ToLower();

        foreach (Transform button in content)
        {
            string name = GetName(button);
            button.gameObject.SetActive(search == "" || name.StartsWith(search));
        }
    }

    string GetName(Transform button)
    {
        string result = "";
        foreach (TMP_Text t in button.GetComponentsInChildren<TMP_Text>(true))
        {
            foreach (char c in t.text)
            {
                if (char.IsLetter(c)) result += c;
            }
        }
        return result.ToLower();
    }
}