using TMPro;
using UnityEngine;

public class ElementListUI : MonoBehaviour
{
    public static ElementListUI Instance;

    [Header("References")]
    public Transform contentParent;   // the ElementListContent object
    public GameObject rowPrefab;      // the ElementRow prefab

    void Awake()
    {
        Instance = this;
    }

    public void AddRow(string elementName)
    {
        GameObject row = Instantiate(rowPrefab, contentParent);
        row.GetComponent<TMP_Text>().text = elementName;
    }

    public void ClearAll()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
    }
}