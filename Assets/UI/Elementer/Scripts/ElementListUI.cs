using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ElementListUI : MonoBehaviour
{
    public static ElementListUI Instance;

    [Header("References")]
    public Transform contentParent;   // the ElementListContent object
    public GameObject rowPrefab;      // the ElementRow prefab

    private Dictionary<string, TMP_Text> activeRows = new Dictionary<string, TMP_Text>();
    private Dictionary<string, int> counts = new Dictionary<string, int>();

    void Awake()
    {
        Instance = this;
    }
    public void RemoveRow(string elementName)
    {
        if (!activeRows.ContainsKey(elementName))
            return;

        counts[elementName]--;

        if (counts[elementName] <= 0)
        {
            Destroy(activeRows[elementName].gameObject);
            activeRows.Remove(elementName);
            counts.Remove(elementName);
        }
        else
        {
            activeRows[elementName].text = $"{counts[elementName]}x {elementName}";
        }
    }

    public void AddRow(string elementName)
    {
        if (activeRows.ContainsKey(elementName))
        {
            counts[elementName]++;
        }
        else
        {
            GameObject row = Instantiate(rowPrefab, contentParent);
            TMP_Text text = row.GetComponent<TMP_Text>();

            activeRows[elementName] = text;
            counts[elementName] = 1;
        }

        activeRows[elementName].text = $"{counts[elementName]}x {elementName}";
    }

    public void ClearAll()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
        activeRows.Clear();
        counts.Clear();
    }

    // NEW: lets other scripts read what's currently selected
    public Dictionary<string, int> GetCounts()
    {
        return counts;
    }
}