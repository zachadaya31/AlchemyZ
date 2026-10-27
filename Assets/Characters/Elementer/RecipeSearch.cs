using UnityEngine;
using TMPro;

public class RecipeSearch : MonoBehaviour
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

        foreach (Transform row in content)
        {
            TMP_Text label = row.GetComponentInChildren<TMP_Text>(true);
            if (label == null) continue;

            row.gameObject.SetActive(search == "" || label.text.ToLower().Contains(search));
        }
    }
}