using System;
using System.Collections;
using UnityEngine;

public class HitMarkerUIManager : MonoBehaviour
{
    public static HitMarkerUIManager Instance { get; private set; }

    [SerializeField] private CanvasGroup hitMarkerCanvasGroup;
    [SerializeField, Range(0, 1)] private float maxOpacity = .75f;
    [SerializeField, Min(0)] private float holdTime = .5f;
    [SerializeField, Min(0)] private float fadeTime = .5f;

    private Coroutine _updateCoroutine;

    private void Awake()
    {
        // Set the alpha of the hit marker canvas group to 0
        hitMarkerCanvasGroup.alpha = 0; 
    }

    private void OnEnable()
    {
        Instance = this;

        // Set the alpha of the hit marker canvas group to 0
        hitMarkerCanvasGroup.alpha = 0;
    }

    private void OnDisable()
    {
        // Stop the update coroutine
        StopCoroutine(_updateCoroutine);
        
        hitMarkerCanvasGroup.alpha = 0;
    }

    private IEnumerator UpdateCoroutine(float lastHitTime, bool isCritical)
    {
        // Calculate start time for the fade
        var fadeStartTime = lastHitTime + holdTime;
        
        var fadeEndTime = fadeStartTime + fadeTime;
        
        while (Time.time < fadeEndTime)
        {
            // Get the inverse lerp value
            var lerpValue = Mathf.InverseLerp(fadeStartTime, fadeEndTime, Time.time);
            
            // Lerp the alpha of the hit marker canvas group
            hitMarkerCanvasGroup.alpha = Mathf.Lerp(maxOpacity, 0, lerpValue);
            
            yield return null;
        }
        
        // Set the alpha to 0
        hitMarkerCanvasGroup.alpha = 0;
    }
    
    public void ShowHitMarker(bool isCritical)
    {
        // If there is an active coroutine, stop it
        if (_updateCoroutine != null)
            StopCoroutine(_updateCoroutine);
        
        _updateCoroutine = StartCoroutine(UpdateCoroutine(Time.time, isCritical));
    }
}