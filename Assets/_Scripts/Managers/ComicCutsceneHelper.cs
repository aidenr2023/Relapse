using System;
using UnityEngine;

public class ComicCutsceneHelper : MonoBehaviour
{
    [SerializeField, Range(0, 1)] private float timeScale = 1;

    private void Update()
    {
        // Set the time scale to the desired time scale
        Time.timeScale = timeScale;
    }
    
    public void SetTimeScale(float newTimeScale)
    {
        // Set the time scale to the new time scale
        timeScale = newTimeScale;
    }
}