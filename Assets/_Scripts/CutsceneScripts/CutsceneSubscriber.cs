using System;
using UnityEngine;
using System.Collections;
using Cinemachine;

public class CutsceneSubscriber : MonoBehaviour
{
    #region References
    
    [SerializeField] private GameObject _player;
    private CutsceneHandler _cutsceneHandler;
    private PlayerMovementV2 _playerMovementV2;
    private PlayerLook _playerCameraMovement;
    private Animator _playerCutsceneAnimator;
    private WeaponManager _weaponManager;
    
    [Header("References")]
    
    [Header("Player Reference")]
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private Animator _playerAnimator;

    [Header("Rotation Settings")]
    [SerializeField] private float _rotationResetDelay = 0.1f;
    [SerializeField] private float _maxRotationCheckTime = 2f;
    private Quaternion _initialRotation;

    [Header("Rotation Validation")] 
    [SerializeField] private float _checkInterval = 0.3f;
    [SerializeField] private float _maxCheckDuration = 3f;
    [SerializeField] private float _angleThreshold = 1f;

    #endregion

    [Tooltip("Reset player rotation to (0,0,0) instead of stored rotation?")] 
    [SerializeField] private bool resetRotationToZero = true;

    private Coroutine _rotationCheckCoroutine;
    private Quaternion _storedRotation;

    private void Start()
    {
        _initialRotation = _playerTransform.rotation;
        InitializeComponents();
        SetupCutsceneListeners();
    }

    private void InitializeComponents()
    {
        _playerMovementV2 = GetComponent<PlayerMovementV2>();
        _playerCameraMovement = GetComponent<PlayerLook>();
        _weaponManager = GetComponent<WeaponManager>();
        _playerCutsceneAnimator = GetComponent<Animator>();

        if (CutsceneManager.Instance != null)
        {
            CutsceneManager.Instance.RegisterPlayer(_playerCutsceneAnimator);
            _cutsceneHandler = CutsceneManager.Instance.CutsceneHandler;
        }
        else
        {
            Debug.LogError("CutsceneManager not found");
        }
    }

    private void SetupCutsceneListeners()
    {
        if (_cutsceneHandler == null) return;

        _cutsceneHandler.OnCutsceneStart.AddListener(OnCutsceneStart);
        _cutsceneHandler.OnCutsceneEnd.AddListener(OnCutsceneEnd);
    }
    
    
    /// <summary>
    /// Invoked when a cutscene starts. checks if the player needs to be moved or not
    /// and if the cutscene is first person or not
    /// </summary>
    private void OnCutsceneStart()
    {
        _initialRotation = _playerTransform.rotation;
    
        bool needsMovement = _cutsceneHandler.IsPlayerMovementNeeded;
        bool isFirstPerson = _cutsceneHandler.IsCutsceneFirstPerson;

        // Early exit for first-person cutscenes with active movement
        if (needsMovement && isFirstPerson)
            return;

        // Handle movement-restricted scenarios
        if (!needsMovement)
        {
            if (isFirstPerson)
            {
                DisablePlayerSystemsFirstPerson();
            }
            else
            {
                DisablePlayerSystems();
            }
        }
    }
    
    
    /// <summary>
    /// Invoked when a cutscene ends. checks if the player had movement or not
    /// and if the cutscene is first person or not to enable the player systems
    /// </summary>
    private void OnCutsceneEnd()
    {
        bool isFirstPerson = _cutsceneHandler.IsCutsceneFirstPerson;
        bool needsMovement = _cutsceneHandler.IsPlayerMovementNeeded;

        // Handle camera and core systems first
        if (!isFirstPerson)
        {
            // Third-person cutscene (both movement needed and not needed cases)
            EnablePlayerSystems();
        }
        else if (!needsMovement)
        {
            // First-person cutscene without movement needed
            EnablePlayerSystemsFirstPerson();
        }

        // Handle common post-cutscene cleanup
        HandleRotationValidation();
    
        // Uncomment if needed
        // StopScriptedEvents();
    }

    private void HandleRotationValidation()
    {
        if (_rotationCheckCoroutine != null)
            StopCoroutine(_rotationCheckCoroutine);
    
        _rotationCheckCoroutine = StartCoroutine(ValidatePlayerRotation());
    }

    /// <summary>
    /// function to disable player systems for third person cutscenes
    /// </summary>
    private void DisablePlayerSystems()
    {
        //on cutscene start set the player inactive
        _player.SetActive(false);
        
        // _storedRotation = _playerTransform.rotation;
        // _playerCameraMovement.enabled = false;
        // _playerMovementV2.DisablePlayerControls();
        // _weaponManager.enabled = false;
        Debug.Log("Player character disabled");
    }
    
    /// <summary>
    /// function to enable player systems for third person cutscenes
    /// </summary>
    private void EnablePlayerSystems()
    {
        //on cutscene end set the player active
        _player.SetActive(true);
        // _playerTransform.rotation = resetRotationToZero ? Quaternion.identity : _storedRotation;
        // _playerCameraMovement.enabled = true;
        // _playerMovementV2.EnablePlayerControls();
        // _weaponManager.enabled = true;
        Debug.Log("Player character enabled");
    }
    
    /// <summary>
    /// Function to enable player systems for first person cutscenes
    /// </summary>
    private void EnablePlayerSystemsFirstPerson()
    {
        _playerTransform.rotation = resetRotationToZero ? Quaternion.identity : _storedRotation;
        _playerCameraMovement.enabled = true;
        _playerMovementV2.EnablePlayerControls();
        _weaponManager.enabled = true;
        Debug.Log("Player systems enabled");
    }
    
    /// <summary>
    /// function to disable player systems for first person cutscenes
    /// </summary>
    private void DisablePlayerSystemsFirstPerson()
    {
        _storedRotation = _playerTransform.rotation;
        _playerCameraMovement.enabled = false;
        _playerMovementV2.DisablePlayerControls();
        _weaponManager.enabled = false;
        Debug.Log("Player systems disabled");
    }
    
    private IEnumerator ValidatePlayerRotation()
    {
    
        // Wait for final animation frame
        yield return new WaitForEndOfFrame();
        
        // Disable animator to stop animation overrides
        if (_playerAnimator != null)
        {
            _playerAnimator.enabled = false;
        }
        
        float elapsedTime = 0f;
        bool needsReset = true;
    
        float elapsed = 0f;
        while (elapsed < _maxRotationCheckTime)
        {
            // Directly set rotation and ignore animations
            _playerTransform.rotation = _initialRotation;
            
            // Force immediate physics update
            Physics.SyncTransforms();
            
            // Check if rotation stuck
            if (Quaternion.Angle(_playerTransform.rotation, _initialRotation) < 0.001f)
            {
                break;
            }

            elapsed += _rotationResetDelay;
            yield return new WaitForSeconds(_rotationResetDelay);
        }

        // Final guarantee
        _playerTransform.rotation = _initialRotation;
        
        // Re-enable components if needed
        if (_playerAnimator != null)
        {
            _playerAnimator.enabled = true;
        }
    }

    private bool IsRotationValid(Vector3 currentRotation)
    {
        return Mathf.Abs(currentRotation.y) > 0.1f || Mathf.Abs(currentRotation.y) < 0.1f;
    }

    private void PlayScriptedEvents()
    {
        // Implement scripted event logic
    }

    private void StopScriptedEvents()
    {
        // Implement scripted event cleanup
    }

    private void OnDestroy()
    {
        if (_cutsceneHandler == null) return;
        
        _cutsceneHandler.OnCutsceneStart.RemoveListener(OnCutsceneStart);
        _cutsceneHandler.OnCutsceneEnd.RemoveListener(OnCutsceneEnd);
    }
}