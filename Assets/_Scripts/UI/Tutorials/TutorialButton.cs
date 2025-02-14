using TMPro;
using UnityEngine;

public class TutorialButton : MonoBehaviour
{
    [SerializeField] private TMP_Text tutorialNameText;
    [SerializeField] private Tutorial tutorial;

    private PauseMenuManager _pauseMenuManager;
    
    public void Initialize(PauseMenuManager pauseMenuManager, Tutorial tutorial)
    {
        _pauseMenuManager = pauseMenuManager;
        this.tutorial = tutorial;
        tutorialNameText.text = tutorial.TutorialName;
    }

    public void OnPress()
    {
        // Play the tutorial
        TutorialScreen.Play(_pauseMenuManager, tutorial);
    }
}