using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneObjectList : MonoBehaviour
{
    // Assign these via the inspector.
    [SerializeField] private List<GameObject> objectsToToggle;

    private void OnEnable()
    {
        // Subscribe to cutscene events when the object is enabled.
        if (CutsceneManager.Instance != null && CutsceneManager.Instance.CutsceneHandler != null)
        {
            CutsceneManager.Instance.CutsceneHandler.OnCutsceneStart.AddListener(EnableObjects);
            CutsceneManager.Instance.CutsceneHandler.OnCutsceneEnd.AddListener(DisableObjects);
        }
    }

    private void OnDisable()
    {
        // Unsubscribe from events when the object is disabled.
        if (CutsceneManager.Instance != null && CutsceneManager.Instance.CutsceneHandler != null)
        {
            CutsceneManager.Instance.CutsceneHandler.OnCutsceneStart.RemoveListener(DisableObjects);
            CutsceneManager.Instance.CutsceneHandler.OnCutsceneEnd.RemoveListener(EnableObjects);
        }
    }

    private void DisableObjects()
    {
        // Disable each GameObject in the list when the cutscene starts.
        foreach (GameObject obj in objectsToToggle)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }

    private void EnableObjects()
    {
        // Enable each GameObject in the list when the cutscene ends.
        foreach (GameObject obj in objectsToToggle)
        {
            if (obj != null)
                obj.SetActive(true);
        }
    }
}
