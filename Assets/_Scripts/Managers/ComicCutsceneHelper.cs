using System;
using UnityEngine;

public class ComicCutsceneHelper : MonoBehaviour
{
    [SerializeField, Range(0, 1)] private float timeScale = 1;

    private TokenManager<float>.ManagedToken _timeToken;

    private void Awake()
    {
        // Create the time token
        _timeToken = TimeScaleManager.Instance.TimeScaleTokenManager.AddToken(1, -1, true);
    }

    private void OnDestroy()
    {
        // Remove the time token
        TimeScaleManager.Instance.TimeScaleTokenManager.RemoveToken(_timeToken);
    }

    private void Update()
    {
        // Update the time scale
        UpdateTimeScale();
    }

    private void UpdateTimeScale()
    {
        _timeToken.Value = timeScale;
        
        if (TimeScaleManagerHelper.Instance != null)
            return;

        // Manually update the time scale if the manager does not exist
        Time.timeScale = _timeToken.Value;
    }

    public void SetTimeScale(float newTimeScale)
    {
        // Set the time scale to the new time scale
        timeScale = newTimeScale;
    }
}