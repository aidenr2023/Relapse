using System;
using UnityEngine;
using System.Collections;

public class CutsceneSubscriber : MonoBehaviour
{
    #region References

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

    private void OnCutsceneStart()
    {
        _initialRotation = _playerTransform.rotation;
        
        if (!_cutsceneHandler.IsPlayerMovementNeeded)
        {
            DisablePlayerSystems();
        }
        else
        {
            PlayScriptedEvents();
        }
    }

    private void OnCutsceneEnd()
    {
        
        if (!_cutsceneHandler.IsPlayerMovementNeeded)
        {
            if (_rotationCheckCoroutine != null)
            {
                StopCoroutine(_rotationCheckCoroutine);
            }
            _rotationCheckCoroutine = StartCoroutine(ValidatePlayerRotation());
            EnablePlayerSystems();
        }
        else
        {
            StopScriptedEvents();
        }
    }

    private void DisablePlayerSystems()
    {
        _storedRotation = _playerTransform.rotation;
        _playerCameraMovement.enabled = false;
        _playerMovementV2.DisablePlayerControls();
        _weaponManager.enabled = false;
        Debug.Log("Player systems disabled");
    }

    private void EnablePlayerSystems()
    {
        _playerTransform.rotation = resetRotationToZero ? Quaternion.identity : _storedRotation;
        _playerCameraMovement.enabled = true;
        _playerMovementV2.EnablePlayerControls();
        _weaponManager.enabled = true;
        Debug.Log("Player systems enabled");
    }

    private void StartRotationValidation()
    {
        if (_rotationCheckCoroutine != null)
            StopCoroutine(_rotationCheckCoroutine);
        
        _rotationCheckCoroutine = StartCoroutine(ValidatePlayerRotation());
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