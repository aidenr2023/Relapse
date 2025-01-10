using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;

public class TutorialScreen : GameMenu, IUsesInput
{
    public static TutorialScreen Instance { get; private set; }

    #region Serialized Fields

    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text subtitleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text pageText;

    [SerializeField] private Tutorial debugTutorial;

    #endregion

    #region Private Fields

    private Tutorial _currentTutorial;

    private int _currentTutorialPage;

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

    protected override void CustomActivate()
    {
        // Register the input
        InputManager.Instance.Register(this);
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
        if (Input.GetKeyDown(KeyCode.T))
            PlayTutorial(debugTutorial);

        // if (IsActive)
        // {
        //     if (Input.GetKeyDown(KeyCode.RightArrow))
        //         NextPage();
        //     else if (Input.GetKeyDown(KeyCode.LeftArrow))
        //         PreviousPage();
        // }
    }

    public override void OnBackPressed()
    {
        Deactivate();
    }

    public void ChangeTutorial(Tutorial tutorial)
    {
        var previousTutorial = _currentTutorial;
        _currentTutorial = tutorial;

        // If the tutorial is the same as the previous tutorial, return
        if (_currentTutorial == previousTutorial && IsActive)
            return;

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

    public void PlayTutorial(Tutorial tutorial)
    {
        // Return if the tutorial is null
        if (tutorial == null)
            return;

        ChangeTutorial(tutorial);
        Activate();

        // Get the instance of the player tutorial manager & complete the tutorial
        Player.Instance.PlayerTutorialManager.CompleteTutorial(tutorial);
    }

    #region IUsesInput

    public HashSet<InputData> InputActions { get; } = new();

    public void InitializeInput()
    {
        InputActions.Add(
            new InputData(InputManager.Instance.DefaultInputActions.UI.Navigate, InputType.Performed,
                OnNavigatePerformed)
        );
    }

    private void OnNavigatePerformed(InputAction.CallbackContext obj)
    {
        // Return if not active
        if (!IsActive)
            return;

        var value = obj.ReadValue<Vector2>();

        const float threshold = 0.5f;

        if (value.x > threshold)
            NextPage();
        else if (value.x < -threshold)
            PreviousPage();
    }

    #endregion
}