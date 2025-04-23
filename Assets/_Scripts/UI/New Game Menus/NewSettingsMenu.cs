using System;
using UnityEngine;
using UnityEngine.UI;

public class NewSettingsMenu : MonoBehaviour
{
    public static NewSettingsMenu Instance { get; private set; }
    
    #region Serialized Fields

    [SerializeField] private SettingsHelper settingsHelper;
    [SerializeField] private NewGameMenu gameMenu;
    
    [Header("Settings Menu"), SerializeField]
    private Button backButton;

    [SerializeField] private Button saveButton;
    [SerializeField] private Button resetButton;

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

    #region Private Fields

    private UIControls _uiControls;
    private Button _currentPaneButton;

    #endregion

    private void Awake()
    {
        // Create a new UIControls object
        _uiControls = new UIControls();
        
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
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

    #region Public Methods

    public void Activate()
    {
        gameMenu.Activate();
    }
    
    public void CustomActivate()
    {
        // Copy the settings from the user settings to the menu settings
        settingsHelper.InitializeMenuSettings();

        // Enable the UI controls
        _uiControls.Enable();

        // Initialize the navigation
        InitializeNavigation();
    }

    public void CustomDeactivate()
    {
        // Disable the UI controls
        _uiControls.Disable();
    }

    public void UpdatePaneNavigation(SettingsMenuContentPane pane)
    {
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
        
        // Update the reset button's navigation
        var oldResetButtonNavigation = resetButton.navigation;
        resetButton.navigation = new Navigation()
        {
            mode = Navigation.Mode.Explicit,
            selectOnUp = backUpNav,
            selectOnDown = _currentPaneButton,
            selectOnLeft = oldResetButtonNavigation.selectOnLeft,
            selectOnRight = oldResetButtonNavigation.selectOnRight,
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

    public void SetCurrentPaneButton(Button button)
    {
        _currentPaneButton = button;
    }
    
    #endregion
}