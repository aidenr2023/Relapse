using System;
using System.Collections;
using UnityEngine;

public class UIJitter : MonoBehaviour
{
    [SerializeField] private float fps = 12;
    [SerializeField] private bool playWhilePaused = false;
    [SerializeField, Range(0, 1)] private float jitterLerpAmount = 1;

    [SerializeField] private float minJitterRotation = -10f;
    [SerializeField] private float maxJitterRotation = 10f;

    [SerializeField] private float minJitterScale = 0.9f;
    [SerializeField] private float maxJitterScale = 1.1f;


    private float _currentJitterRotation;
    private float _currentJitterScale;

    private Coroutine _jitterCoroutine;
    private bool _isRunning;

    private void OnEnable()
    {
        // Set the running flag to true
        _isRunning = true;

        // Start the jitter when the object is enabled
        _jitterCoroutine = StartCoroutine(Jitter());
    }

    private void OnDisable()
    {
        // Stop the jitter when the object is disabled
        StopCoroutine(_jitterCoroutine);
        _jitterCoroutine = null;
        _isRunning = false;

        // Remove the jitter when the object is disabled
        RemoveJitter();
    }

    private IEnumerator Jitter()
    {
        while (_isRunning)
        {
            // Return if the game is paused
            if (MenuManager.Instance.IsGamePausedInMenus && !playWhilePaused)
            {
                yield return new WaitForSecondsRealtime(1f / fps);
                continue;
            }
            
            // Remove the jitter rotation
            RemoveJitter();

            // Randomize the jitter
            RandomizeJitter();

            // Apply the jitter 
            ApplyJitter();

            // Wait for the next frame
            yield return new WaitForSecondsRealtime(1f / fps);
        }
    }

    private void RandomizeJitter()
    {
        // Randomize the rotation
        _currentJitterRotation = UnityEngine.Random.Range(
            minJitterRotation * jitterLerpAmount,
            maxJitterRotation * jitterLerpAmount
        );

        var minScale = 1 + (minJitterScale - 1) * jitterLerpAmount;
        var maxScale = 1 + (maxJitterScale - 1) * jitterLerpAmount;

        // Randomize the scale
        _currentJitterScale = UnityEngine.Random.Range(minScale, maxScale);
    }

    private void ApplyJitter()
    {
        var localRotation = transform.localRotation.eulerAngles;

        // Apply the jitter rotation
        transform.localRotation = Quaternion.Euler(
            localRotation.x,
            localRotation.y,
            localRotation.z + _currentJitterRotation
        );

        var localScale = transform.localScale;

        if (_currentJitterScale != 0)
            transform.localScale = localScale * _currentJitterScale;
    }

    private void RemoveJitter()
    {
        var localRotation = transform.localRotation.eulerAngles;

        // Remove the current jitter rotation
        transform.localRotation = Quaternion.Euler(
            localRotation.x,
            localRotation.y,
            localRotation.z - _currentJitterRotation
        );

        var localScale = transform.localScale;

        // Remove the current jitter scale
        if (_currentJitterScale != 0)
            transform.localScale = localScale * (1 / _currentJitterScale);
    }

    public void SetLerpAmount(float lerpAmount)
    {
        jitterLerpAmount = Mathf.Clamp01(lerpAmount);
    }
}