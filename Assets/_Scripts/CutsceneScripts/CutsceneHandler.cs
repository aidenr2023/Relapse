using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Events;

/// <summary>
/// Plays a cutscene based on the provided CutsceneData and signals its start and end via UnityEvents.
/// </summary>
public class CutsceneHandler : MonoBehaviour
{
    [SerializeField] private PlayableDirector playableDirector;
    
    // UnityEvents that notify listeners when a cutscene starts and ends.
    public UnityEvent OnCutsceneStart;
    public UnityEvent OnCutsceneEnd;
    
    /// <summary>
    /// Plays the cutscene defined in the CutsceneData.
    /// </summary>
    /// <param name="cutsceneData">The data asset containing the timeline and camera info.</param>
    public void PlayCutscene(CutsceneData cutsceneData)
    {
        if (playableDirector == null || cutsceneData == null)
        {
            Debug.LogError("PlayableDirector or CutsceneData is missing.");
            return;
        }

        // Set the timeline asset to play.
        playableDirector.playableAsset = cutsceneData.timelineAsset;
        
        // Subscribe to the 'stopped' event to know when the cutscene finishes.
        playableDirector.stopped += OnCutsceneStopped;
        
        // Notify listeners that the cutscene is starting.
        OnCutsceneStart?.Invoke();
        
        // Play the cutscene.
        playableDirector.Play();
    }
    
    /// <summary>
    /// Callback invoked when the cutscene finishes.
    /// </summary>
    private void OnCutsceneStopped(PlayableDirector director)
    {
        playableDirector.stopped -= OnCutsceneStopped;
        // Notify listeners that the cutscene has ended.
        OnCutsceneEnd?.Invoke();
    }
}