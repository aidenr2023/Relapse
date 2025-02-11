using System;
using System.Collections.Generic;
using UnityEngine;

public class MusicObject : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private MusicType musicType;
    [SerializeField] private GameMenu parentMenu;

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
        audioSource.Pause();
    }

    private void ResumeMusic()
    {
        audioSource.UnPause();
    }

    private enum MusicType
    {
        Game,
        Menu,
    }
}