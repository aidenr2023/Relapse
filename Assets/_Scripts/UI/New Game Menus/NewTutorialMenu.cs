using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class NewTutorialMenu : MonoBehaviour
{
    private const string TUTORIAL_SCENE_NAME = "TutorialUIScene";

    private static NewTutorialMenu Instance { get; set; }

    #region Serialized Fields

    [SerializeField] private TutorialArrayVariable allTutorials;
    [SerializeField] private TutorialArrayVariable completedTutorials;

    [SerializeField] private NewGameMenu gameMenu;

    [SerializeField, Min(0)] private float slowDownTime = 1;

    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text subtitleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text pageText;
    [SerializeField] private Button exitButton;
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

    private DefaultInputActions _inputActions;
    
    private Tutorial _currentTutorial;

    private int _currentTutorialPage;

    private bool _navigateNeedsToReset;

    private bool _hasReachedEnd;

    private float _currentSlowTime;

    private bool _isActive;

    #endregion

    #region Getters

    private Tutorial CurrentTutorial => _currentTutorial;

    #endregion

    #region Initialization Methods

    private void Awake()
    {
        // Set the instance to this
        Instance = this;
        
        // Initialize the input
        InitializeInput();

        // Disable the video player
        videoPlayer.enabled = false;
    }

    private void InitializeInput()
    {
        // Create a new input actions instance
        _inputActions = new DefaultInputActions();
        
        _inputActions.UI.Navigate.performed += OnNavigatePerformed;
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
        if (!_isActive)
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


    private void OnDestroy()
    {
        // Set the instance to null
        Instance = null;
    }

    #endregion

    #region Public Methods

    public void CustomActivate()
    {
        // // Register the input
        // InputManager.Instance.Register(this);

        // Set the selected game object to the exit button
        EventSystem.current.SetSelectedGameObject(null);

        // enabled the video
        CoroutineBuilder
            .Create()
            .WaitSecondsRealtime(.125f)
            .Enqueue(() => videoPlayer.enabled = true)
            .Start(this);

        // Activate the menu
        _isActive = true;
        
        // Enable the controls
        _inputActions.Enable();
    }

    public void CustomDeactivate()
    {
        // Stop the video
        videoPlayer.Stop();

        var routine = CoroutineBuilder
            .Create()
            .Enqueue(() => videoPlayer.enabled = false);
        // .Enqueue(() => InputManager.Instance.Unregister(this));

        if (_isActive)
        {
            routine
                .WaitSecondsRealtime(.125f)
                .Enqueue(ResumeGame(_currentSlowTime));
        }

        // routine.Start(this);
        routine.Start(Player.Instance);

        // Deactivate the menu
        _isActive = false;
        
        // Disable the controls
        _inputActions.Disable();
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

    public static void Play(MonoBehaviour script, Tutorial tutorial, bool replay = true)
    {
        // Run the coroutine
        script.StartCoroutine(CreateTutorialSceneAndPlay(tutorial, replay));
    }

    public static void Play(MonoBehaviour script, Tutorial tutorial, float slowTime, bool replay = true)
    {
        // Run the coroutine
        script.StartCoroutine(CreateTutorialSceneAndPlay(tutorial, slowTime, replay));
    }

    #endregion

    #region Private Methods

    #region UI Methods

    private void SetVideoClip(VideoClip videoClip)
    {
        // Return if the video clip is the same as the current video clip
        if (videoClip == videoPlayer.clip && _isActive)
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
        EventSystem.current.SetSelectedGameObject(exitButton.gameObject);
    }

    #endregion

    private void PlayTutorial(Tutorial tutorial, bool replay = true)
    {
        PlayTutorial(tutorial, slowDownTime, replay);
    }

    private void PlayTutorial(Tutorial tutorial, float slowTime, bool replay = true)
    {
        // Return if the tutorial is null
        if (tutorial == null)
            return;

        var isTutorialCompleted = completedTutorials.value.Contains(tutorial);

        // Return if the tutorial has already been completed and we are not replaying it
        if (isTutorialCompleted && !replay)
            return;

        // Start the coroutine
        StartCoroutine(TutorialCoroutine(tutorial, replay, isTutorialCompleted, slowTime));
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

    private void ChangeTutorial(Tutorial tutorial)
    {
        var previousTutorial = _currentTutorial;
        _currentTutorial = tutorial;

        // If the tutorial is the same as the previous tutorial, return
        if (_currentTutorial == previousTutorial && _isActive)
            return;

        // Reset the has reached end flag
        _hasReachedEnd = false;

        // Reset the current tutorial page
        SetTutorialPage(0);
    }

    private IEnumerator TutorialCoroutine(Tutorial tutorial, bool replay, bool isTutorialCompleted, float slowTime)
    {
        if (!replay && isTutorialCompleted)
            yield break;

        // Set the current slow time
        _currentSlowTime = slowTime;

        // Create a time scale token
        var timeToken = TimeScaleManager.Instance.TimeScaleTokenManager.AddToken(1, -1, true);

        var startTime = Time.unscaledTime;

        while (slowTime > 0 && Time.unscaledTime - startTime < slowTime)
        {
            timeToken.Value = 1 - Mathf.Clamp01((Time.unscaledTime - startTime) / slowTime);

            yield return null;
        }

        // Remove the time token
        TimeScaleManager.Instance.TimeScaleTokenManager.RemoveToken(timeToken);

        ChangeTutorial(tutorial);
        gameMenu.Activate();

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
        completedTutorials.value.Add(tutorial);

        yield return null;
    }

    private IEnumerator ResumeGame(float slowTime)
    {
        // Create a new time scale token
        var timeToken = TimeScaleManager.Instance.TimeScaleTokenManager.AddToken(0, -1, true);

        var startTime = Time.unscaledTime;

        while (slowTime != 0 && Time.unscaledTime - startTime < slowTime)
        {
            timeToken.Value = Mathf.Clamp01((Time.unscaledTime - startTime) / slowTime);

            yield return null;
        }

        // Remove the time token
        TimeScaleManager.Instance.TimeScaleTokenManager.RemoveToken(timeToken);
    }

    private static IEnumerator CreateTutorialSceneAndPlay(Tutorial tutorial, float slowTime, bool replay = true)
    {
        // If the instance is NOT null, just play the tutorial
        if (Instance != null)
        {
            Instance.PlayTutorial(tutorial, slowTime, replay);
            yield break;
        }

        // Load the tutorial scene
        var asyncOperation = SceneManager.LoadSceneAsync(TUTORIAL_SCENE_NAME, LoadSceneMode.Additive);

        // Wait until the scene is loaded
        yield return new WaitUntil(() => asyncOperation.isDone);

        var tutorialScene = SceneManager.GetSceneByName(TUTORIAL_SCENE_NAME);

        // Get the tutorial screen instance
        var tutorialMenu = FindObjectsOfType<NewTutorialMenu>()
            .FirstOrDefault(n => n.gameObject.scene == tutorialScene);

        // Play the tutorial
        tutorialMenu?.PlayTutorial(tutorial, replay);
    }

    private static IEnumerator CreateTutorialSceneAndPlay(Tutorial tutorial, bool replay = true)
    {
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
        var tutorialMenu = FindObjectsOfType<NewTutorialMenu>()
            .FirstOrDefault(n => n.gameObject.scene == tutorialScene);

        // Play the tutorial
        tutorialMenu?.PlayTutorial(tutorial, replay);
    }

    #endregion
}