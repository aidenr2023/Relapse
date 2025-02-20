using UnityEngine;

/// <summary>
/// Listens to cutscene events from the CutsceneHandler and disables or enables player movement and UI accordingly.
/// </summary>
public class CutsceneSubscriber : MonoBehaviour
{
    #region References
    public CutsceneHandler cutsceneHandler;
    PlayerMovementV2 playerMovementV2;
    private PlayerActions playerActions;
    // Reference to the player's UI.
    public GameObject playerUI;
    private PlayerLook playerCameraMovement;
    private WeaponManager weaponManager;
    private Quaternion storedRotation;
    #endregion References
    
    private void Start()
    {
        //Get the player's PlayerMovementV2 component
        playerMovementV2 = GetComponent<PlayerMovementV2>();
        
        // Get the player's PlayerLook component.
        playerCameraMovement = GetComponent<PlayerLook>();

        // Get the player's WeaponManager component.
        weaponManager = GetComponent<WeaponManager>();

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
        //get rotation of player and store it 
        storedRotation = playerMovementV2.transform.rotation;
    
        playerMovementV2.DisablePlayerControls();
        weaponManager.enabled = false;
        
    }

    /// <summary>
    /// Re-enables player movement when the cutscene ends.
    /// </summary>
    public void EnableMovement()
    {
        playerMovementV2.transform.rotation = storedRotation;
        
        playerMovementV2.EnablePlayerControls();
       
        weaponManager.enabled = true;
        
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
