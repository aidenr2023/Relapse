using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.Animations;

[RequireComponent(typeof(PlayableDirector))]
public class CutsceneHandler : MonoBehaviour
{
    public UnityEvent OnCutsceneStart;
    public UnityEvent OnCutsceneEnd;

    private PlayableDirector _director;
    private Animator _playerCutsceneAnimator;
    private bool _isCutsceneActive;
    public bool IsPlayerMovementNeeded { get;  set; }
    public bool IsCutsceneFirstPerson { get;  set; }
    
    public enum CutsceneType
    {
        FirstPerson,
        ThirdPerson
    }
        
    private void Start()
    {
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
            Debug.Log($"[HANDLER] PlayerAnimator registered: {CutsceneManager.Instance.PlayerCutsceneAnimator != null}");
            _playerCutsceneAnimator = CutsceneManager.Instance.PlayerCutsceneAnimator;
        }

        if (_playerCutsceneAnimator == null)
        {
            Debug.LogError("[HANDLER] Player Animator not found!");
        }
    }
    
    /// <summary>
    /// Plays a cutscene using the provided timeline asset and conditions
    /// </summary>
    /// <param name="timelineAsset"></param>
    /// <param name="isMovementNeeded"></param>
    public void PlayCutscene(PlayableAsset timelineAsset, bool isMovementNeeded, CutsceneType perspective)
    {
        if (_isCutsceneActive) return;

        if (!ValidateDependencies(timelineAsset)) return;
        IsCutsceneFirstPerson = (perspective == CutsceneType.FirstPerson);
        IsPlayerMovementNeeded = isMovementNeeded;
        
        StartCoroutine(PlayCutsceneDelayed(timelineAsset));
    }

    private IEnumerator PlayCutsceneDelayed(PlayableAsset timelineAsset)
    {
        yield return new WaitUntil((() => CutsceneManager.Instance.PlayerCutsceneAnimator 
        != null && _playerCutsceneAnimator));
        
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
        
        if (IsCutsceneFirstPerson)
        {
            BindFirstPersonAnimatorTracks();
        }
        else
        {
            Debug.Log("Using prebinded third person animator tracks...");
        }
    }
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

    private void StartCutscene()
    {
        _isCutsceneActive = true;
        _director.stopped += OnCutsceneFinished;

        #if !UNITY_EDITOR
        _director.timeUpdateMode = DirectorUpdateMode.GameTime;
        #endif
        
        OnCutsceneStart?.Invoke();
        //log the start of the cutscene
        Debug.Log("Cutscene started Event invoked");
        _director.Play();
    }

    private void OnCutsceneFinished(PlayableDirector director)
    {
        _isCutsceneActive = false;
        _director.stopped -= OnCutsceneFinished;
        OnCutsceneEnd?.Invoke();
        
        // Optional: Reset timeline bindings
        _director.playableAsset = null;
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