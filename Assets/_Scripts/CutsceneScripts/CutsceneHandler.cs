using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Events;
using UnityEngine.Animations;

[RequireComponent(typeof(PlayableDirector))]
public class CutsceneHandler : MonoBehaviour
{
    public UnityEvent OnCutsceneStart;
    public UnityEvent OnCutsceneEnd;

    private PlayableDirector _director;
    private Animator _playerCutsceneAnimator;
    private bool _isCutsceneActive;
    

    private void Awake()
    {
        _director = GetComponent<PlayableDirector>();
        InitializePlayerReferences();
    }

    private void InitializePlayerReferences()
    {
        if (CutsceneManager.Instance != null && CutsceneManager.Instance.PlayerController != null)
        {
            _playerCutsceneAnimator = CutsceneManager.Instance.PlayerController.PlayerAnimator;
        }
    
        if (_playerCutsceneAnimator == null)
        {
            Debug.LogWarning("Player animator reference not found on initialization. Will attempt dynamic binding.");
        }
    }

    public void PlayCutscene(PlayableAsset timelineAsset)
    {
        if (_isCutsceneActive) return;

        if (!ValidateDependencies(timelineAsset)) return;

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
            CutsceneManager.Instance.PlayerController != null)
        {
            return CutsceneManager.Instance.PlayerController.PlayerAnimator;
        }

        var playerObj = GameObject.FindGameObjectWithTag("Player");
        return playerObj?.GetComponentInChildren<Animator>();
    }

    private void ConfigureTimeline(PlayableAsset timelineAsset)
    {
        _director.playableAsset = timelineAsset;
        
        foreach (var output in _director.playableAsset.outputs)
        {
            if (output.outputTargetType == typeof(Animator))
            {
                _director.SetGenericBinding(output.sourceObject, _playerCutsceneAnimator);
            }
        }
    }

    private void StartCutscene()
    {
        _isCutsceneActive = true;
        _director.stopped += OnCutsceneFinished;
        OnCutsceneStart?.Invoke();
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