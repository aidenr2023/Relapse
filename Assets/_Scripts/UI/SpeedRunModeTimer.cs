using System;
using TMPro;
using UnityEngine;

[ExecuteAlways]
public class SpeedRunModeTimer : MonoBehaviour
{
    [SerializeField] private string bestTimeTextString = "Best Time: ";
    [SerializeField] private string currentTimeTextString = "Current Time: ";
    
    [Space, SerializeField] private TMP_Text bestTimeText;
    [SerializeField] private TMP_Text currentTimeText;
    
    [Space, SerializeField] private FloatVariable speedRunModeBestTime;
    [SerializeField] private FloatVariable speedRunModeCurrentTime;
    
    private void SetTimes(float bestTime, float currentTime)
    {
        // Set the text of the best time and current time
        bestTimeText.text = $"{bestTimeTextString}{SecondsToTimeString(bestTime)}";
        currentTimeText.text = $"{currentTimeTextString}{SecondsToTimeString(currentTime)}";
    }

    private void Update()
    {
        // Set the times if the variables are set
        SetTimes(speedRunModeBestTime.value, speedRunModeCurrentTime.value);
    }
    
    private static string SecondsToTimeString(float time)
    {
        // Convert the time to a TimeSpan
        var timeSpan = TimeSpan.FromSeconds(time);
        
        // Return the time as a string
        return $"{timeSpan.Minutes:00}:{timeSpan.Seconds + (timeSpan.Milliseconds / 1000f):00.00}";
    }
}