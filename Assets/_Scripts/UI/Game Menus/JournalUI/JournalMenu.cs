using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class JournalMenu : GameMenu
{
    private const string JOURNAL_SCENE_NAME = "JournalUIScene";

    #region Serialized Fields

    [SerializeField] private GameObject objectiveParent;
    [SerializeField] private GameObject inventoryParent;
    [SerializeField] private GameObject powersParent;
    [SerializeField] private GameObject memoriesParent;
    
    [SerializeField] private GameObject firstSelectedButton;

    #endregion
        
    #region Private Fields

    private bool _inputtedThisFrame;

    #endregion

    #region Getters

    public static JournalMenu Instance { get; private set; }

    public bool IsPaused { get; private set; }

    #endregion

    protected override void CustomAwake()
    {
        // Set the instance to this
        Instance = this;
    }

    protected override void CustomStart()
    {
    }

    protected override void CustomDestroy()
    {
        Deactivate();
        
        // Unset the instance if it is this
        if (Instance == this)
            Instance = null;
    }

    protected override void CustomActivate()
    {
        // Populate the objectives menu

        // Populate the items menu

        // Populate the powers menu

        // Populate the memories menu
    }

    protected override void CustomDeactivate()
    {
    }

    protected override void CustomUpdate()
    {
        // Reset the inputted this frame flag
        _inputtedThisFrame = false;

        if (eventSystem.currentSelectedGameObject == null)
            SetSelectedButton(firstSelectedButton);
    }

    public override void OnBackPressed()
    {
        // Return if inputted this frame
        if (_inputtedThisFrame)
            return;

        // If the game is not paused, return
        if (!IsPaused)
            return;

        _inputtedThisFrame = true;

        // Resume the game
        Resume();
    }

    public void OnJournalPerformed(InputAction.CallbackContext obj)
    {
        // Return if inputted this frame
        if (_inputtedThisFrame)
            return;

        _inputtedThisFrame = true;

        TogglePause();
    }

    private void TogglePause()
    {
        // If there are any other active menus, don't toggle the pause menu
        if (MenuManager.Instance.ActiveMenus.Any(n => n != this))
            return;

        // Pause the game
        if (!IsPaused)
            Pause();

        // Resume the game
        else
            Resume();
    }
    
    public void Pause()
    {
        // Un-hide the pause menu
        // pauseMenuParent.SetActive(true);
        Activate();

        // Set pause to true
        IsPaused = true;

        // Isolate the pause menu
        IsolateMenu(objectiveParent);
    }
    
    /// <summary>
    /// Called when the Resume button is clicked.
    /// </summary>
    public void Resume()
    {
        // Hide menu
        Deactivate();

        // Set pause to false
        IsPaused = false;
    }
    
    public void IsolateMenu(GameObject obj)
    {
        // Hide all the menus
        objectiveParent?.SetActive(false);
        inventoryParent?.SetActive(false);
        powersParent?.SetActive(false);
        memoriesParent?.SetActive(false);

        // Show the selected menu
        obj?.SetActive(true);
    }

    public void SetSelectedButton(GameObject button)
    {
        eventSystem.SetSelectedGameObject(button);
    }

    public static IEnumerator LoadJournalMenu()
    {
        // Load the vendor UI scene
        SceneManager.LoadScene(JOURNAL_SCENE_NAME, LoadSceneMode.Additive);

        // Wait while the instance is null
        yield return new WaitUntil(() => Instance != null);
    }
}