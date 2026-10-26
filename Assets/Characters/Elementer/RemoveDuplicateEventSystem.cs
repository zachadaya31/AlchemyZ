using UnityEngine;
using UnityEngine.EventSystems;

public class RemoveDuplicateEventSystem : MonoBehaviour
{
    void Awake()
    {
        // if Mission1 already has one, this copy removes itself
        if (FindObjectsByType<EventSystem>().Length > 1)
        {
            Destroy(gameObject);
        }
    }
}