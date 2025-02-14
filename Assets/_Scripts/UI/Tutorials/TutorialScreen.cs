using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using Object = System.Object;

public class TutorialScreen : GameMenu, IUsesInput
{
    private const string TUTORIAL_SCENE_NAME = "TutorialUIScene";

    private static TutorialScreen Instance { get; set; }

    #region Serialized Fields

    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text subtitleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text pageText;
    [SerializeField] private Button exitButton;
    [SerializeField] private CanvasGroup exitCanvasGroup;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button prevButton;
    
    [Header("Tutorial Buttons")] [SerializeField]
    private GameObject buttonsParent;

    [SerializeField] private TutorialButtonManager shootButton;

    [SerializeField] private TutorialButtonManager reloadButton;
    [SerializeField] private TutorialButtonManager interactButton;
    [SerializeField] private TutorialButtonManager usePowerButton;
    [SerializeField] private TutorialButtonManager changePowerButton;

    [SerializeField] private TutorialButtonManager moveButton;
    [SerializeField] private TutorialButtonManager sprintButton;
    [SerializeField] private TutorialButtonManager jumpButton;
    [SerializeField] private TutorialButtonManager slideButton;

    [SerializeField] private TutorialButtonManager pauseButton;
    [SerializeField] private TutorialButtonManager journalButton;

    #endregion

    #region Private Fields

    private Tutorial _currentTutorial;

    private int _currentTutorialPage;

    private bool _navigateNeedsToReset;

    private bool _hasReachedEnd;

    #endregion

    #region Getters

    public Tutorial CurrentTutorial => _currentTutorial;
    public TutorialPage CurrentTutorialPage => _currentTutorial.TutorialPages[_currentTutorialPage];

    #endregion

    protected override void CustomAwake()
    {
        // Set the instance to this
        Instance = this;

        // Initialize the input
        InitializeInput();
    }

    protected override void CustomStart()
    {
    }

    protected override void CustomActivate()
    {
        // Register the input
        InputManager.Instance.Register(this);
        
        // Set the selected game object to the exit button
        eventSystem.SetSelectedGameObject(null);
    }

    protected override void CustomDeactivate()
    {
        // Stop the video
        videoPlayer.Stop();

        // Unregister the input
        InputManager.Instance.Unregister(this);
    }

    protected override void CustomUpdate()
    {
        if (CurrentTutorial == null) 
            return;
        
        // Set the button image to the current tutorial page
        SetButtonImage(CurrentTutorial.TutorialPages[_currentTutorialPage].Button);

        // If the exit button is active & the current selected element is null,
        // set the exit button as the selected game object
        if (exitButton.gameObject.activeSelf && eventSystem.currentSelectedGameObject == null)
            eventSystem.SetSelectedGameObject(exitButton.gameObject);
    }

    protected override void CustomDestroy()
    {
        // Set the instance to null
        Instance = null;
    }

    public override void OnBackPressed()
    {
        // Return if the exit button is not active
        if (!_hasReachedEnd)
            return;
        
        Deactivate();
    }

    public void ChangeTutorial(Tutorial tutorial)
    {
        var previousTutorial = _currentTutorial;
        _currentTutorial = tutorial;

        // If the tutorial is the same as the previous tutorial, return
        if (_currentTutorial == previousTutorial && IsActive)
            return;
        
        // Reset the has reached end flag
        _hasReachedEnd = false;

        // Reset the current tutorial page
        SetTutorialPage(0);
    }

    private void SetVideoClip(VideoClip videoClip)
    {
        // Return if the video clip is the same as the current video clip
        if (videoClip == videoPlayer.clip && IsActive)
            return;

        // Set the video clip to the video player
        videoPlayer.clip = videoClip;

        // Restart the video
        videoPlayer.Stop();
        videoPlayer.Play();
    }

    private void SetTitleText(string title)
    {
        // Set the title text to the title
        titleText.text = title;
    }

    private void SetSubtitleText(string subtitle)
    {
        // Set the subtitle text to the subtitle
        subtitleText.text = subtitle;
    }

    private void SetDescriptionText(string description)
    {
        // Set the description text to the description
        descriptionText.text = description;
    }

    private void SetPageText(int page, int totalPages)
    {
        // Set the page text to the page and total pages
        pageText.text = $"Page {page + 1} / {totalPages}";
    }

    private void SetButtonImage(TutorialPage.TutorialButton tutorialButton)
    {
        // Set all the images to inactive
        shootButton.HideImages();
        reloadButton.HideImages();
        interactButton.HideImages();
        usePowerButton.HideImages();
        changePowerButton.HideImages();
        moveButton.HideImages();
        sprintButton.HideImages();
        jumpButton.HideImages();
        slideButton.HideImages();
        pauseButton.HideImages();
        journalButton.HideImages();

        var isKeyboard = InputManager.Instance.CurrentControlScheme == InputManager.ControlSchemeType.Keyboard;

        // Enable the buttons parent
        buttonsParent.SetActive(true);

        switch (tutorialButton)
        {
            case TutorialPage.TutorialButton.None:
                // Disable the buttons parent
                buttonsParent.SetActive(false);
                break;

            case TutorialPage.TutorialButton.Shoot:
                shootButton.SetActiveButton(isKeyboard);
                break;

            case TutorialPage.TutorialButton.Reload:
                reloadButton.SetActiveButton(isKeyboard);
                break;

            case TutorialPage.TutorialButton.Interact:
                interactButton.SetActiveButton(isKeyboard);
                break;

            case TutorialPage.TutorialButton.UsePower:
                usePowerButton.SetActiveButton(isKeyboard);
                break;

            case TutorialPage.TutorialButton.ChangePower:
                changePowerButton.SetActiveButton(isKeyboard);
                break;

            case TutorialPage.TutorialButton.Move:
                moveButton.SetActiveButton(isKeyboard);
                break;

            case TutorialPage.TutorialButton.Sprint:
                sprintButton.SetActiveButton(isKeyboard);
                break;

            case TutorialPage.TutorialButton.Jump:
                jumpButton.SetActiveButton(isKeyboard);
                break;

            case TutorialPage.TutorialButton.Slide:
                slideButton.SetActiveButton(isKeyboard);
                break;

            case TutorialPage.TutorialButton.Pause:
                pauseButton.SetActiveButton(isKeyboard);
                break;

            case TutorialPage.TutorialButton.Journal:
                journalButton.SetActiveButton(isKeyboard);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(tutorialButton), tutorialButton, null);
        }
    }

    private void SetTutorialPage(int index)
    {
        _currentTutorialPage = index;

        // Set the title text
        SetTitleText(CurrentTutorial.TutorialName);

        // Set the video clip to the video
        SetVideoClip(CurrentTutorial.TutorialPages[index].VideoClip);

        // Set the subtitle text
        SetSubtitleText(CurrentTutorial.TutorialPages[index].SubTitle);

        // Set the description text
        SetDescriptionText(CurrentTutorial.TutorialPages[index].Description);

        // Set the page text
        SetPageText(index, CurrentTutorial.TotalPages);

        // Set the button image
        SetButtonImage(CurrentTutorial.TutorialPages[index].Button);
        
        // Update the exit button
        UpdateExitButton();
    }

    public void NextPage()
    {
        // If the current tutorial page is the last page, return
        if (_currentTutorialPage >= _currentTutorial.TutorialPages.Count - 1)
            return;

        // Increment the current tutorial page
        SetTutorialPage(_currentTutorialPage + 1);
    }

    public void PreviousPage()
    {
        // If the current tutorial page is the first page, return
        if (_currentTutorialPage <= 0)
            return;

        // Decrement the current tutorial page
        SetTutorialPage(_currentTutorialPage - 1);
    }

    public void PlayTutorial(Tutorial tutorial, bool replay = true)
    {
        // Return if the tutorial is null
        if (tutorial == null)
            return;

        var isTutorialCompleted = TutorialManager.Instance.IsTutorialCompleted(tutorial);
        
        // Return if the tutorial has already been completed and we are not replaying it
        if (isTutorialCompleted && !replay)
            return;

        ChangeTutorial(tutorial);
        Activate();

        // Hacky solution to force the exit button to pop up if the tutorial has already been completed
        if (replay && isTutorialCompleted)
        {
            // Set the current page to the last page
            SetTutorialPage(tutorial.TutorialPages.Count - 1);
            
            // Force an update of the exit button
            UpdateExitButton();
            
            // Reset the current page to the first page
            SetTutorialPage(0);
        }

        // Get the instance of the player tutorial manager & complete the tutorial
        Player.Instance.PlayerTutorialManager.CompleteTutorial(tutorial);
    }

    public static void Play(MonoBehaviour script, Tutorial tutorial, bool replay = true)
    {
        // Run the coroutine
        script.StartCoroutine(CreateTutorialSceneAndPlay(tutorial, replay));
    }

    private static IEnumerator CreateTutorialSceneAndPlay(Tutorial tutorial, bool replay = true)
    {
        // If the tutorial has already been completed and we are not replaying it, return
        if (Player.Instance.PlayerTutorialManager.HasCompletedTutorial(tutorial) && !replay)
            yield break;

        // If the instance is NOT null, just play the tutorial
        if (Instance != null)
        {
            Instance.PlayTutorial(tutorial, replay);
            yield break;
        }

        // Load the tutorial scene
        var asyncOperation = SceneManager.LoadSceneAsync(TUTORIAL_SCENE_NAME, LoadSceneMode.Additive);

        // Wait until the scene is loaded
        yield return new WaitUntil(() => asyncOperation.isDone);

        var tutorialScene = SceneManager.GetSceneByName(TUTORIAL_SCENE_NAME);

        // Get the tutorial screen instance
        var tutorialScreen = FindObjectsOfType<TutorialScreen>()
            .FirstOrDefault(n => n.gameObject.scene == tutorialScene);

        // Play the tutorial
        tutorialScreen?.PlayTutorial(tutorial, replay);
    }

    private void UpdateExitButton()
    {
        if (!_hasReachedEnd)
            exitButton.gameObject.SetActive(false);
        
        // If the current tutorial page is not the last page, return
        if (_currentTutorialPage < _currentTutorial.TutorialPages.Count - 1)
            return;
        
        // If the flag is already set, return
        if (_hasReachedEnd)
            return;
        
        // Set the flag to true
        _hasReachedEnd = true;
        
        // Set the exit button to active
        exitButton.gameObject.SetActive(true);

        // Select the exit button
        eventSystem.SetSelectedGameObject(exitButton.gameObject);
    }
    
    #region IUsesInput

    public HashSet<InputData> InputActions { get; } = new();

    public void InitializeInput()
    {
        InputActions.Add(
            new InputData(InputManager.Instance.DefaultInputActions.UI.Navigate, InputType.Performed,
                OnNavigatePerformed)
        );

        InputActions.Add(
            new InputData(InputManager.Instance.DefaultInputActions.UI.Navigate, InputType.Canceled,
                OnNavigateCanceled)
        );
    }

    private void OnNavigatePerformed(InputAction.CallbackContext obj)
    {
        var value = obj.ReadValue<Vector2>();

        const float threshold = 0.5f;

        if (Mathf.Abs(value.x) < threshold)
        {
            _navigateNeedsToReset = false;
            return;
        }

        // Return if not active
        if (!IsActive)
            return;

        // Return if the navigate needs to reset
        if (_navigateNeedsToReset)
            return;

        // Set the navigate needs to reset flag to true
        _navigateNeedsToReset = true;

        if (value.x > threshold)
            NextPage();
        else if (value.x < -threshold)
            PreviousPage();
    }

    private void OnNavigateCanceled(InputAction.CallbackContext obj)
    {
        // Set the navigate needs to reset flag to false
        _navigateNeedsToReset = false;
    }

    #endregion
}