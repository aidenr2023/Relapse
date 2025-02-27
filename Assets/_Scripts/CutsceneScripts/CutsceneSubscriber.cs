using System;
using UnityEngine;
using System.Collections;

/// <summary>
/// Listens to cutscene events from the CutsceneHandler and disables or enables player movement and UI accordingly.
/// </summary>
public class CutsceneSubscriber : MonoBehaviour
{
    #region References
    private CutsceneHandler _cutsceneHandler;
    PlayerMovementV2 _playerMovementV2;
    private PlayerActions _playerActions;
    public GameObject playerUI;
    private PlayerLook _playerCameraMovement;
    private Animator _playerCutsceneAnimator;
    private WeaponManager _weaponManager;
    private readonly Quaternion _storedRotation = Quaternion.identity;
    //[SerializeField] CutsceneTrigger cutsceneTrigger;
    [SerializeField]private GameObject _playerTransform;
    #endregion

    public Animator PlayerCutsceneAnimatorRef => _playerCutsceneAnimator;
    [Tooltip("Reset player rotation to (0,0,0) instead of stored rotation?")]
    [SerializeField] private bool resetRotationToZero = true;

    private void Awake()
    {
        
    }

    private void Start()
    {
        // Get your references as before.
        _playerMovementV2 = GetComponent<PlayerMovementV2>();
        _playerCameraMovement = GetComponent<PlayerLook>();
        _weaponManager = GetComponent<WeaponManager>();
        _playerCutsceneAnimator = GetComponent<Animator>();
        //get the player gameobject
        _playerTransform = GameObject.FindGameObjectWithTag("Player");

        if (CutsceneManager.Instance != null)
        {
            Debug.Log("[PLAYER Animator] Registering player with CutsceneManager");
            CutsceneManager.Instance.RegisterPlayer(_playerCutsceneAnimator);
            _cutsceneHandler = CutsceneManager.Instance.CutsceneHandler;

        }
        else
        {
            Debug.LogError("CutsceneManager not found");
        }
            // When isPlayerMovementNeeded is false, disable movement.
            if (!_cutsceneHandler.IsPlayerMovementNeeded)
            {
                _cutsceneHandler.OnCutsceneStart.AddListener(DisableListener);
                _cutsceneHandler.OnCutsceneEnd.AddListener(EnableListener);
              //  _cutsceneHandler.OnCutsceneStart.AddListener(DisableUI);
               // _cutsceneHandler.OnCutsceneEnd.AddListener(EnableUI);
            }
            else
            {
                // Optionally handle other cutscene events if player movement remains enabled.
                Debug.Log("Player movement is allowed during cutscene; controls remain enabled.");
                _cutsceneHandler.OnCutsceneStart.AddListener(PlayScriptedEvents);
                _cutsceneHandler.OnCutsceneEnd.AddListener(StopScriptedEvents);
            }
    }

    public void PlayScriptedEvents()
    {
        // Play scripted events here
        
    }
    
    public void StopScriptedEvents()
    {
        // Stop scripted events here
    }
    

    public void DisableListener()
    {
        // Dynamic check of the movement flag
        if (!_cutsceneHandler.IsPlayerMovementNeeded)
        {
            DisableMovement();
            DisableUI();
        }
        else
        {
            PlayScriptedEvents();
        }
    }

    public void EnableListener()
    {
        if (!_cutsceneHandler.IsPlayerMovementNeeded)
        {
            EnableMovement();
            EnableUI();
        }
        else
        {
            StopScriptedEvents();
        }
    }
    
    public void DisableMovement()
    {
        // Store rotation WHEN CUTSCENE STARTS, not at Start()
        // Apply rotation based on the reset flag
        _playerCameraMovement.enabled = false;
        Debug.Log("Stored Rotation: " + _storedRotation);
        _playerMovementV2.DisablePlayerControls();
        _weaponManager.enabled = false;
    }
    
    public void EnableMovement()
    {
        // Restore to stored rotation or zero based on setting
        _playerTransform.transform.rotation = resetRotationToZero 
            ? Quaternion.identity 
            : _storedRotation;
        Debug.Log("Restored Rotation: " + _playerTransform.transform.rotation);
        _playerCameraMovement.enabled = true;
        _playerMovementV2.EnablePlayerControls();
        _weaponManager.enabled = true;
        StartCoroutine(CheckPlayerRotationCoroutine());
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
    
    /// <summary>
    /// Operations to clean up subscriptions and references and rotaion
    /// </summary>
    private IEnumerator CheckPlayerRotationCoroutine(float checkInterval = 5f, float threshold = .0001f)
    {
        // Optional: wait a brief moment after the cutscene ends
        yield return new WaitForSeconds(1.5f);
    
        while (true)
        {
            // Check if the angle between the player's current rotation and zero (Quaternion.identity) is within a small threshold.
            if (Quaternion.Angle(_playerMovementV2.transform.rotation, Quaternion.identity) < threshold)
            {
                Debug.Log("Player rotation is now effectively zero.");
                break;
            }
            else
            {
                Debug.Log("Player rotation not zero yet. Checking again in " + checkInterval + " seconds.");
            }
            yield return new WaitForSeconds(checkInterval);
        }
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
