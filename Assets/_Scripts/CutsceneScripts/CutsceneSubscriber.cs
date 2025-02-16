using UnityEngine;

/// <summary>
/// Listens to cutscene events from the CutsceneHandler and disables or enables player movement and UI accordingly.
/// </summary>
public class CutsceneSubscriber : MonoBehaviour
{
    public CutsceneHandler cutsceneHandler;
    public BasicPlayerMovement playerMovement;
    // Reference to the player's UI.
    public GameObject playerUI;

    private void Start()
    {
        // Subscribe to cutscene start/end events.
        if (cutsceneHandler != null)
        {
            cutsceneHandler.OnCutsceneStart.AddListener(DisableMovement);
            cutsceneHandler.OnCutsceneEnd.AddListener(EnableMovement);
            cutsceneHandler.OnCutsceneStart.AddListener(DisableUI);
            cutsceneHandler.OnCutsceneEnd.AddListener(EnableUI);
        }
        else
        {
            Debug.LogError("CutsceneHandler not assigned in CutsceneSubscriber.");
        }
    }

    /// <summary>
    /// Disables player movement when the cutscene starts.
    /// </summary>
    public void DisableMovement()
    {
        playerMovement.enabled = false;
    }

    /// <summary>
    /// Re-enables player movement when the cutscene ends.
    /// </summary>
    public void EnableMovement()
    {
        playerMovement.enabled = true;
    }
    
    /// <summary>
    /// Disables the player's UI during the cutscene.
    /// </summary>
    public void DisableUI()
    {
        playerUI.SetActive(false);
    }
    
    /// <summary>
    /// Re-enables the player's UI after the cutscene.
    /// </summary>
    public void EnableUI()
    {
        playerUI.SetActive(true);
    }
    
    private void OnDestroy()
    {
        // Clean up subscriptions to prevent memory leaks.
        if (cutsceneHandler != null)
        {
            cutsceneHandler.OnCutsceneStart.RemoveListener(DisableMovement);
            cutsceneHandler.OnCutsceneEnd.RemoveListener(EnableMovement);
            cutsceneHandler.OnCutsceneStart.RemoveListener(DisableUI);
            cutsceneHandler.OnCutsceneEnd.RemoveListener(EnableUI);
        }
    }
}
