using System;
using System.Collections;
using UnityEngine;

public class PlayerRelapseFullscreen : MonoBehaviour
{
    private static readonly int PercentMaterialID = Shader.PropertyToID("_Percent");
    [SerializeField] private PlayerInfoEventVariable playerRelapseStart;
    [SerializeField] private PlayerInfoEventVariable playerRelapseEnd;
    [SerializeField] private FullScreenPassRendererFeature fullScreenPassRendererFeature;

    [SerializeField] private AnimationCurve fadeInCurve;
    [SerializeField] private AnimationCurve fadeOutCurve;
    
    private Coroutine _currentCoroutine;

    private void OnEnable()
    {
        // Subscribe to the events
        playerRelapseStart += OnPlayerRelapseStart;
        playerRelapseEnd += OnPlayerRelapseEnd;
    }

    private void OnDisable()
    {
        // Unsubscribe from the events
        playerRelapseStart -= OnPlayerRelapseStart;
        playerRelapseEnd -= OnPlayerRelapseEnd;
    }

    private void OnPlayerRelapseEnd(PlayerInfo arg0)
    {
        // Stop any existing coroutine
        if (_currentCoroutine != null)
            StopCoroutine(_currentCoroutine);

        // Fade out the effect
        _currentCoroutine = StartCoroutine(Fade(0, false));
    }

    private void OnPlayerRelapseStart(PlayerInfo arg0)
    {
        // Stop any existing coroutine
        if (_currentCoroutine != null)
            StopCoroutine(_currentCoroutine);

        // Fade in the effect
        _currentCoroutine = StartCoroutine(Fade(1, true));
    }

    private IEnumerator Fade(float targetValue, bool inOut)
    {
        Debug.Log($"Fading to {targetValue} ({(inOut ? "In" : "Out")})");
        
        var currentCurve = inOut ? fadeInCurve : fadeOutCurve;
        var fadeDuration = currentCurve.keys[currentCurve.length - 1].time;
        
        var startTime = Time.time;
        
        while (Time.time < startTime + fadeDuration)
        {
            // Calculate the percentage of the fade
            var currentTime = Time.time - startTime;
            var currentValue = currentCurve.Evaluate(currentTime);
            
            // Set the material property
            SetValue(currentValue);
            
            yield return null;
        }
        
        // Set the final value to ensure it reaches the target
        SetValue(targetValue);
    }

    private void SetValue(float value)
    {
        // Set the material property
        fullScreenPassRendererFeature.passMaterial.SetFloat(PercentMaterialID, value);
    }
}