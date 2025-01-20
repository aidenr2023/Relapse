using System;
using UnityEngine;

public class WhispersManager : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private Whisper whisperPrefab;

    [SerializeField] private Transform[] whisperPositions;

    #endregion

    #region Private Fields

    private int _positionIndex = 0;

    #endregion

    #region Getters

    #endregion

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
            AddWhisper("This is a whisper! Whisper Whisper Whisper!", 5);
    }

    public void AddWhisper(string message, float duration)
    {
        // Instantiate a whisper
        var whisper = Instantiate(whisperPrefab, transform);

        // Get a random position
        var randomPosition = whisperPositions[_positionIndex];

        // Increment the position index
        _positionIndex = (_positionIndex + 1) % whisperPositions.Length;

        // Get a random position
        whisper.Initialize(message, duration, randomPosition);
    }
}