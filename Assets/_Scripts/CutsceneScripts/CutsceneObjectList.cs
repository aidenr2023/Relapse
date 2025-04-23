using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneObjectList : MonoBehaviour
{
    // Assign these via the inspector.
    [SerializeField] private List<GameObject> objectsToToggle;
   // [SerializeField] private GameObject player;

    private void OnEnable()
    {
        // Subscribe to cutscene events when the object is enabled.
        if (CutsceneManager.Instance != null && CutsceneManager.Instance.CutsceneHandler != null)
        {
            //CutsceneManager.Instance.CutsceneHandler.OnCutsceneStart.AddListener(EnableObjects);
            //CutsceneManager.Instance.CutsceneHandler.OnCutsceneEnd.AddListener(DisableObjects);
        }
    }

    private void OnDisable()
    {
        // Unsubscribe from events when the object is disabled.
        if (CutsceneManager.Instance != null && CutsceneManager.Instance.CutsceneHandler != null)
        {
            //CutsceneManager.Instance.CutsceneHandler.OnCutsceneStart.RemoveListener(EnableObjects);
            //CutsceneManager.Instance.CutsceneHandler.OnCutsceneEnd.RemoveListener(DisableObjects);
        }
    }

    public void DisableObjects()
    {
        // Disable and destroy each GameObject in the list
        foreach (GameObject obj in objectsToToggle)
        {
            if (obj == null) continue;

            // Disable the object first
            obj.SetActive(false);
            // Then destroy it
            Destroy(obj);
        }
    }

   

    public void EnableObjects()
    {
        // Enable each GameObject in the list when the cutscene start.
        foreach (GameObject obj in objectsToToggle)
        {
            if (obj != null)
                obj.SetActive(true);
        }
    }
    
}
