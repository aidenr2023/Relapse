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
    public GameObject playerUI;
    private PlayerLook playerCameraMovement;
    private WeaponManager weaponManager;
    private Quaternion storedRotation;
    #endregion

    [Tooltip("Reset player rotation to (0,0,0) instead of stored rotation?")]
    [SerializeField] private bool resetRotationToZero = true;

    private void Start()
    {
        playerMovementV2 = GetComponent<PlayerMovementV2>();
        playerCameraMovement = GetComponent<PlayerLook>();
        weaponManager = GetComponent<WeaponManager>();

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

    public void DisableMovement()
    {
        // Store rotation WHEN CUTSCENE STARTS, not at Start()
        //force player rotation to 0,0,0
        storedRotation = playerMovementV2.transform.rotation.eulerAngles.y == 0 ? Quaternion.identity : playerMovementV2.transform.rotation;
        Debug.Log("Stored Rotation: " + storedRotation);

        playerMovementV2.DisablePlayerControls();
        weaponManager.enabled = false;
    }

    public void EnableMovement()
    {
        // Restore to stored rotation or zero based on setting
        playerMovementV2.transform.rotation = resetRotationToZero ? Quaternion.identity : storedRotation;
        Debug.Log("Restored Rotation: " + playerMovementV2.transform.rotation);

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
