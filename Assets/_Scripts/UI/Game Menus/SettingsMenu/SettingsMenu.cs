﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsMenu : GameMenu
{
    public static SettingsMenu Instance { get; private set; }

    #region Serialized Fields

    [SerializeField] private SettingsHelper settingsHelper;

    [Header("Settings Menu"), SerializeField]
    private GameObject firstSelectedButton;

    [SerializeField] private Button backButton;

    [SerializeField] private Button saveButton;

    [Header("Content Panes"), SerializeField]
    private SettingsMenuContentPane generalSettingsPane;

    [SerializeField] private SettingsMenuContentPane videoSettingsPane;
    [SerializeField] private SettingsMenuContentPane audioSettingsPane;
    [SerializeField] private SettingsMenuContentPane inputSettingsPane;
    [SerializeField] private SettingsMenuContentPane accessibilitySettingsPane;
    [SerializeField] private SettingsMenuContentPane extrasSettingsPane;

    [Header("Tab Buttons"), SerializeField]
    private Button generalButton;

    [SerializeField] private Button videoButton;
    [SerializeField] private Button audioButton;
    [SerializeField] private Button inputButton;
    [SerializeField] private Button accessibilityButton;
    [SerializeField] private Button extrasButton;

    #endregion

    private Button _currentPaneButton;

    private UIControls _uiControls;

    protected override void CustomAwake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // Create a new UIControls object
        _uiControls = new UIControls();
    }

    protected override void CustomStart()
    {
    }

    protected override void CustomDestroy()
    {
    }

    protected override void CustomActivate()
    {
        // Copy the settings from the user settings to the menu settings
        settingsHelper.InitializeMenuSettings();

        // Enable the UI controls
        _uiControls.Enable();

        // Initialize the navigation
        InitializeNavigation();

        // Isolate the general settings pane
        IsolatePane(generalSettingsPane);

        // Set the event system's current selected game object to the first selected game object
        StartCoroutine(EnsureSelectObject(firstSelectedButton));
    }

    private void InitializeNavigation()
    {
        generalSettingsPane.SetUpDownNavigation(generalButton);
        videoSettingsPane.SetUpDownNavigation(videoButton);
        audioSettingsPane.SetUpDownNavigation(audioButton);
        inputSettingsPane.SetUpDownNavigation(inputButton);
        accessibilitySettingsPane.SetUpDownNavigation(accessibilityButton);
        extrasSettingsPane.SetUpDownNavigation(extrasButton);
    }

    private IEnumerator EnsureSelectObject(GameObject obj)
    {
        // If this is not the active menu, return
        if (!IsActive)
            yield break;

        while (!eventSystem.enabled || eventSystem.currentSelectedGameObject != obj)
        {
            eventSystem.SetSelectedGameObject(obj);
            yield return null;
        }
    }

    protected override void CustomDeactivate()
    {
        _uiControls.Disable();
    }

    protected override void CustomUpdate()
    {
    }

    public override void OnBackPressed()
    {
        // Deactivate the menu
        Deactivate();
    }

    public void IsolatePane(SettingsMenuContentPane pane)
    {
        // Update the current pane button
        if (pane != null)
        {
            if (pane == generalSettingsPane)
                _currentPaneButton = generalButton;
            else if (pane == videoSettingsPane)
                _currentPaneButton = videoButton;
            else if (pane == audioSettingsPane)
                _currentPaneButton = audioButton;
            else if (pane == inputSettingsPane)
                _currentPaneButton = inputButton;
            else if (pane == accessibilitySettingsPane)
                _currentPaneButton = accessibilityButton;
            else if (pane == extrasSettingsPane)
                _currentPaneButton = extrasButton;
        }

        // Create an array containing all the content panes
        var panes = new[]
        {
            generalSettingsPane,
            videoSettingsPane,
            audioSettingsPane,
            inputSettingsPane,
            accessibilitySettingsPane,
            extrasSettingsPane
        };

        // Loop through all the content panes
        foreach (var currentPane in panes)
        {
            if (currentPane != pane)
                currentPane.Disable();
            else
                currentPane.Enable();
        }

        pane?.SetUpDownNavigation(_currentPaneButton);

        Selectable backUpNav = _currentPaneButton;
        if (pane?.LastItem != null)
            backUpNav = pane.LastItem.GetComponent<Selectable>();

        // Update the back button's navigation
        var oldBackButtonNavigation = backButton.navigation;
        backButton.navigation = new Navigation()
        {
            mode = Navigation.Mode.Explicit,
            selectOnUp = backUpNav,
            selectOnDown = _currentPaneButton,
            selectOnLeft = oldBackButtonNavigation.selectOnLeft,
            selectOnRight = oldBackButtonNavigation.selectOnRight,
        };

        // Update the save button's navigation
        var oldSaveButtonNavigation = saveButton.navigation;
        saveButton.navigation = new Navigation()
        {
            mode = Navigation.Mode.Explicit,
            selectOnUp = backUpNav,
            selectOnDown = _currentPaneButton,
            selectOnLeft = oldSaveButtonNavigation.selectOnLeft,
            selectOnRight = oldSaveButtonNavigation.selectOnRight,
        };

        // Update the last item's navigation
        if (pane?.LastItem != null)
        {
            var lastItemSelectable = pane.LastItem.GetComponent<Selectable>();

            var oldLastItemNavigation = lastItemSelectable.navigation;

            lastItemSelectable.navigation = new Navigation()
            {
                mode = Navigation.Mode.Explicit,
                selectOnUp = oldLastItemNavigation.selectOnUp,
                selectOnDown = backButton,
                selectOnLeft = oldLastItemNavigation.selectOnLeft,
                selectOnRight = oldLastItemNavigation.selectOnRight,
            };
        }
    }
}