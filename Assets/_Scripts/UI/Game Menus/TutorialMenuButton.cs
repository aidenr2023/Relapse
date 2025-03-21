using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialMenuButton : MonoBehaviour
{
    private const float SPAM_COOLDOWN = 3f;

    [SerializeField] private PauseMenuManager pauseMenuManager;

    [SerializeField] private TMP_Text tutorialText;
    [SerializeField] private Image buttonImage;
    [SerializeField] private Button button;

    [SerializeField] private Color availableImageColor = Color.white;
    [SerializeField] private Color unavailableImageColor = new(.25f, .25f, .25f);

    [Space, SerializeField, Readonly] private Tutorial tutorial;

    private bool _isAvailable;

    private bool _isOnSpamCooldown;

    public void SetTutorial(Tutorial newTutorial, bool isAvailable)
    {
        tutorial = newTutorial;
        _isAvailable = isAvailable;

        // Set the tutorial text to the tutorial name
        tutorialText.text = tutorial.TutorialName;

        // If the tutorial isn't available
        // Set the button image color to unavailable
        if (!isAvailable)
            buttonImage.color = unavailableImageColor;

        // Set the button image color to available
        else
            buttonImage.color = availableImageColor;
    }

    public void ResetTutorial()
    {
        // Reset the tutorial
        tutorial = null;

        // Set the button to not interactable
        _isAvailable = false;

        // Clear the tutorial text
        tutorialText.text = "";

        // Set the button image color to unavailable
        buttonImage.color = unavailableImageColor;
    }

    public void PlayTutorial()
    {
        // Return if there is no tutorial
        if (tutorial == null)
            return;

        // Return if the tutorial isn't available
        if (!_isAvailable)
        {
            // Display a tooltip that you haven't unlocked this tutorial yet
            StartCoroutine(DisplayUnavailable());
            return;
        }

        // Play the tutorial
        TutorialScreen.Play(this, tutorial, 0, true);
    }

    private IEnumerator DisplayUnavailable()
    {
        // Return if the button is on spam cooldown
        if (_isOnSpamCooldown)
            yield break;

        // Set the button to on spam cooldown
        _isOnSpamCooldown = true;

        // Display a tooltip that you haven't unlocked this tutorial yet
        JournalTooltipManager.Instance.AddTooltip("You haven't unlocked this tutorial yet!", SPAM_COOLDOWN);

        // Wait for the spam cooldown
        yield return new WaitForSecondsRealtime(SPAM_COOLDOWN);

        _isOnSpamCooldown = false;
    }
}