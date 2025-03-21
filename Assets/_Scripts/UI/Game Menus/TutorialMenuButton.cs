using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialMenuButton : MonoBehaviour
{
    [SerializeField] private PauseMenuManager pauseMenuManager;

    [SerializeField] private TMP_Text tutorialText;
    [SerializeField] private Image buttonImage;
    [SerializeField] private Color availableImageColor = Color.white;
    [SerializeField] private Color unavailableImageColor = new(.25f, .25f, .25f);

    [Space, SerializeField, Readonly] private Tutorial tutorial;

    public void SetTutorial(Tutorial newTutorial)
    {
        tutorial = newTutorial;

        // Set the tutorial text to the tutorial name
        tutorialText.text = tutorial.TutorialName;
    }

    public void ResetTutorial()
    {
        tutorial = null;

        tutorialText.text = "";
    }

    public void PlayTutorial()
    {
        // Return if there is no tutorial
        if (tutorial == null)
            return;
        
        // Play the tutorial
        TutorialScreen.Play(this, tutorial, 0, true);
    }
}