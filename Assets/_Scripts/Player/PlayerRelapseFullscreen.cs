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

    private void Awake()
    {
        // Initialize the material property
        SetValue(0, 0);
    }

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

        var finalPercent = inOut ? 1 : 0;

        while (Time.time < startTime + fadeDuration)
        {
            // Calculate the percentage of the fade
            var currentTime = Time.time - startTime;
            var currentValue = currentCurve.Evaluate(currentTime);

            var currentPercent = Mathf.InverseLerp(startTime, startTime + fadeDuration, currentTime + startTime);

            if (!inOut)
                currentPercent = 1 - currentPercent;

            // Set the material property
            SetValue(currentValue, currentPercent);

            yield return null;
        }

        // Set the final value to ensure it reaches the target
        SetValue(targetValue, finalPercent);
    }

    private void SetValue(float value, float percent)
    {
        // Set the material property
        fullScreenPassRendererFeature.passMaterial.SetFloat(PercentMaterialID, value);

        // Clamp the percent between 0 and 1
        percent = Mathf.Clamp01(percent);
        EnemyRelapseOutlineManager.SetOutlineLerpAmount(percent);
    }
}