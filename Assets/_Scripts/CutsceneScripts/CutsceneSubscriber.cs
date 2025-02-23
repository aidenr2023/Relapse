using System;
using UnityEngine;

/// <summary>
/// Listens to cutscene events from the CutsceneHandler and disables or enables player movement and UI accordingly.
/// </summary>
public class CutsceneSubscriber : MonoBehaviour
{
    #region References
    private CutsceneHandler _cutsceneHandler;
    PlayerMovementV2 playerMovementV2;
    private PlayerActions playerActions;
    public GameObject playerUI;
    private PlayerLook playerCameraMovement;
    private Animator _playerCutsceneAnimator;
    private WeaponManager weaponManager;
    private Quaternion storedRotation;
    #endregion

    public Animator PlayerCutsceneAnimatorRef => _playerCutsceneAnimator;
    [Tooltip("Reset player rotation to (0,0,0) instead of stored rotation?")]
    [SerializeField] private bool resetRotationToZero = true;

    private void Awake()
    {
        
    }

    private void Start()
    {
    
        playerMovementV2 = GetComponent<PlayerMovementV2>();
        playerCameraMovement = GetComponent<PlayerLook>();
        weaponManager = GetComponent<WeaponManager>();
    
        _playerCutsceneAnimator = GetComponent<Animator>();
        if (CutsceneManager.Instance != null)
        {
            Debug.Log("[PLAYER Animator] Registering player with CutsceneManager");
            CutsceneManager.Instance.RegisterPlayer(_playerCutsceneAnimator);
        }
        else
        {
            Debug.LogError("CutsceneManager not found");
        }
        
        if (CutsceneManager.Instance != null)
        {
            _cutsceneHandler = CutsceneManager.Instance.CutsceneHandler;
        }

        if (_cutsceneHandler != null)
        {
            _cutsceneHandler.OnCutsceneStart.AddListener(DisableMovement);
            _cutsceneHandler.OnCutsceneEnd.AddListener(EnableMovement);
            _cutsceneHandler.OnCutsceneStart.AddListener(DisableUI);
            _cutsceneHandler.OnCutsceneEnd.AddListener(EnableUI);
        }
        else
        {
            Debug.LogError("CutsceneHandler not assigned in CutsceneSubscriber.");
        }
    }

    public void DisableMovement()
    {
        // Store rotation WHEN CUTSCENE STARTS, not at Start()
        // Apply rotation based on the reset flag
        
        Debug.Log("Stored Rotation: " + storedRotation);
        storedRotation = playerMovementV2.transform.rotation;
        playerMovementV2.DisablePlayerControls();
        weaponManager.enabled = false;
    }

    public void EnableMovement()
    {
        // Restore to stored rotation or zero based on setting
        playerMovementV2.transform.rotation = resetRotationToZero 
            ? Quaternion.identity 
            : storedRotation;
        Debug.Log("Restored Rotation: " + playerMovementV2.transform.rotation);

        playerMovementV2.EnablePlayerControls();
        weaponManager.enabled = true;
    }

    /// <summary>
    /// Disables the player's UI during the cutscene.
    /// </summary>
    public void DisableUI()
    {
       // playerUI.SetActive(false);
    }
    
    /// <summary>
    /// Re-enables the player's UI after the cutscene.
    /// </summary>
    public void EnableUI()
    {
       // playerUI.SetActive(true);
    }
    
    private void OnDestroy()
    {
        // Clean up subscriptions to prevent memory leaks.
        if (_cutsceneHandler != null)
        {
            _cutsceneHandler.OnCutsceneStart.RemoveListener(DisableMovement);
            _cutsceneHandler.OnCutsceneEnd.RemoveListener(EnableMovement);
            _cutsceneHandler.OnCutsceneStart.RemoveListener(DisableUI);
            _cutsceneHandler.OnCutsceneEnd.RemoveListener(EnableUI);
        }
    }
}
