using System;
using System.Collections.Generic;
using UnityEngine;

public class MusicObject : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private MusicType musicType;
    [SerializeField] private GameMenu parentMenu;
    [SerializeField] private bool restartWhenResuming;

    private bool _isPlaying = false;

    private void Awake()
    {
        // If the audio source is set to play on awake, set the flag to true
        _isPlaying = audioSource.playOnAwake;
    }

    private void Update()
    {
        // Automatically pause the music if necessary
        AutomaticallyPauseMusic();
    }

    private void AutomaticallyPauseMusic()
    {
        var menuManager = MenuManager.Instance;
        var gameMusicNeedsToStop = menuManager.IsGameMusicPausedInMenus;

        switch (musicType)
        {
            case MusicType.Game:
            {
                if (gameMusicNeedsToStop)
                    PauseMusic();

                else
                    ResumeMusic();

                break;
            }

            case MusicType.Menu:
            {
                // If the parent menu is not active, pause the music
                if (parentMenu != null && !parentMenu.IsActive)
                    PauseMusic();

                else
                    ResumeMusic();

                break;
            }
        }
    }

    private void PauseMusic()
    {
        // Return if the music is already paused
        if (!_isPlaying)
            return;

        audioSource.Pause();

        // Set the flag to false
        _isPlaying = false;
    }

    private void ResumeMusic()
    {
        // Return if the music is already playing
        if (_isPlaying)
            return;

        if (restartWhenResuming)
            audioSource.time = 0;

        audioSource.UnPause();

        // Set the flag to true
        _isPlaying = true;
    }

    private enum MusicType
    {
        Game,
        Menu,
    }
}