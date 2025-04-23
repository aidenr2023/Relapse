using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Animations;
using Cinemachine;
using UnityEngine.Rendering.Universal;


[RequireComponent(typeof(PlayableDirector))]
public class CutsceneHandler : MonoBehaviour
{
    public UnityEvent OnCutsceneStart;
    public UnityEvent OnCutsceneEnd;

    private PlayableDirector _director;
    private Animator _playerCutsceneAnimator;
    
    
    //cinemachine brain
    [SerializeField]private CinemachineBrain _cmBrain;
    [SerializeField] private Camera _mainCamera;
    private List<Camera> _overlayCameras;
    int _baseCameraMask;
    private List<int> _overlayCameraMasks;
    
    private bool _isCutsceneActive;
    public bool IsPlayerMovementNeeded { get; set; }
    public bool IsCutsceneFirstPerson { get; set; }

    public enum CutsceneType
    {
        FirstPerson,
        ThirdPerson
    }

    private void Awake()
    {
       
    }

    private void Start()
    {
        //grab the main camera from cinemachine brain
        _mainCamera = _cmBrain.OutputCamera;
        
        //store the base cam culling mask
        _baseCameraMask = _mainCamera.cullingMask;
        
        //grab the overlay cameras
        var camera_data = _mainCamera.GetUniversalAdditionalCameraData();
        _overlayCameras = camera_data.cameraStack;
        
        //store each ones culling mask
        _overlayCameraMasks = _overlayCameras.Select(cam => cam.cullingMask).ToList();
        
        
        _director = GetComponent<PlayableDirector>();
        Debug.Log($"CutsceneHandler initialized with director: {_director}");
        InitializePlayerReferences();
    }

    private void InitializePlayerReferences()
    {
        Debug.Log("[HANDLER] Initializing player references...");

        if (CutsceneManager.Instance != null)
        {
            Debug.Log($"[HANDLER] CutsceneManager exists: {CutsceneManager.Instance != null}");
            Debug.Log(
                $"[HANDLER] PlayerAnimator registered: {CutsceneManager.Instance.PlayerCutsceneAnimator != null}");
            _playerCutsceneAnimator = CutsceneManager.Instance.PlayerCutsceneAnimator;
        }

        if (_playerCutsceneAnimator == null)
            Debug.LogError("[HANDLER] Player Animator not found!");
    }

    /// <summary>
    /// Plays a cutscene using the provided timeline asset and conditions
    /// </summary>
    /// <param name="timelineAsset"></param>
    /// <param name="isMovementNeeded"></param>
    public void PlayCutscene(PlayableAsset timelineAsset, bool isMovementNeeded, CutsceneType perspective)
    {
        if (_isCutsceneActive)
            return;

        if (!ValidateDependencies(timelineAsset))
            return;

        IsCutsceneFirstPerson = (perspective == CutsceneType.FirstPerson);
        IsPlayerMovementNeeded = isMovementNeeded;

        StartCoroutine(PlayCutsceneDelayed(timelineAsset));
    }

    private IEnumerator PlayCutsceneDelayed(PlayableAsset timelineAsset)
    {
        yield return new WaitUntil(() =>
            CutsceneManager.Instance.PlayerCutsceneAnimator != null && _playerCutsceneAnimator);

        ConfigureTimeline(timelineAsset);
        StartCutscene();
    }

    private bool ValidateDependencies(PlayableAsset timelineAsset)
    {
        if (_director == null)
        {
            Debug.LogError("Missing PlayableDirector component!");
            return false;
        }

        if (timelineAsset == null)
        {
            Debug.LogError("No timeline asset provided!");
            return false;
        }

        if (_playerCutsceneAnimator == null)
        {
            _playerCutsceneAnimator = FindPlayerAnimator();
            if (_playerCutsceneAnimator == null)
            {
                Debug.LogError("Failed to locate player animator!");
                return false;
            }
        }

        return true;
    }

    private Animator FindPlayerAnimator()
    {
        if (CutsceneManager.Instance != null &&
            CutsceneManager.Instance.PlayerCutsceneAnimator != null)
        {
            return CutsceneManager.Instance.PlayerCutsceneAnimator;
        }

        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
        {
            Debug.LogError("Player object not found in scene!");
            return null;
        }
        else
        {
            Debug.Log($"Player object found by tag {playerObj.name}");
        }

        return playerObj.GetComponent<Animator>();
    }

    private void ConfigureTimeline(PlayableAsset timelineAsset)
    {
        Debug.Log($"[Cutscene] Timeline Asset: {timelineAsset != null}");
        Debug.Log($"[Cutscene] Director: {_director != null}");
        Debug.Log($"[Cutscene] Animator: {_playerCutsceneAnimator != null}");

        _director.playableAsset = timelineAsset;

        if (!IsCutsceneFirstPerson)
        {
            Debug.Log("Using prebinded third person animator tracks...");
            BindCinemachineBrain();
        }
        else
        {
            BindFirstPersonAnimatorTracks();
        }
    }

    //lowkey obsolete, but needed for Movement2 (Brian Level) cutscene
    private void BindFirstPersonAnimatorTracks()
    {
        foreach (var output in _director.playableAsset.outputs)
        {
            if (output.outputTargetType == typeof(Animator))
            {
                _director.SetGenericBinding(output.sourceObject, _playerCutsceneAnimator);
                Debug.Log($"Bound FP animator to track: {output.streamName}");
            }
        }
    }
    
    //funtion to bind cinemachine brain to the cutscene
    private void BindCinemachineBrain()
    {
        foreach (var output in _director.playableAsset.outputs)
        {
            if (output.outputTargetType == typeof(CinemachineBrain))
            {
                _director.SetGenericBinding(output.sourceObject, _cmBrain);
                Debug.Log($"Bound Cinemachine Brain to track: {output.streamName}");
            }
        }
    }
    
    

    private void StartCutscene()
    {
        _isCutsceneActive = true;
        int hideMask = 1 << LayerMask.NameToLayer("GunHandHolder");    
        
        _mainCamera.cullingMask &= ~hideMask;
        //for each overlay camera, hide the gun hand holder layer
        for (int i = 0; i < _overlayCameras.Count; i++)
            _overlayCameras[i].cullingMask &= ~hideMask;
        
        //disable player body layer
        _director.stopped += OnCutsceneFinished;

#if !UNITY_EDITOR
        _director.timeUpdateMode = DirectorUpdateMode.GameTime;
#endif

        OnCutsceneStart?.Invoke();
        
        // log the start of the cutscene
        Debug.Log("Cutscene started Event invoked");
        _director.Play();
        
        // Hide the UI elements
        GameUIHelper.Instance?.AddUIHider(this);
        
        // Remove control from the player
        (Player.Instance.PlayerController as PlayerMovementV2)!.DisablePlayerControls(this);
        
        
    }

    private void OnCutsceneFinished(PlayableDirector director)
    {
        _isCutsceneActive = false;
        
        // restore the original culling mask
        _mainCamera.cullingMask = _baseCameraMask;
        // foreach overlay camera, restore the original culling mask
        for (int i = 0; i < _overlayCameras.Count; i++)
        {
            _overlayCameras[i].cullingMask = _overlayCameraMasks[i];
        }
        
        
        _director.stopped -= OnCutsceneFinished;
        OnCutsceneEnd?.Invoke();

        // Optional: Reset timeline bindings
        _director.playableAsset = null;
        
        Debug.Log("Cutscene finished Event invoked");
        
        // Remove this as a UI hider
        GameUIHelper.Instance?.RemoveUIHider(this);
        
        // Give control back to the player
        (Player.Instance.PlayerController as PlayerMovementV2)!.EnablePlayerControls(this);
    }

    public void EmergencyStop()
    {
        if (_isCutsceneActive)
        {
            _director.Stop();
            OnCutsceneFinished(_director);
        }
    }
}