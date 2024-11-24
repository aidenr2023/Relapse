using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{
    public static ButtonManager Instance { get; private set; }

    private float tempTimeScale;
    private float fixedDeltaTime;
    [SerializeField] public GameObject PauseMenu;

    // Colors for normal, hover, and click states
    public Color normalColor = Color.white;
    public Color hoverColor = Color.yellow;
    public Color clickColor = Color.red;

    private bool _isPaused;

    public bool IsPaused => _isPaused;

    /// <summary>
    /// Changes the text color of the TextMeshPro to the hover color.
    /// Attach this to the PointerEnter event.
    /// </summary>
    private void Awake()
    {
        // Set the instance to this object
        Instance = this;
    }

    private void Start()
    {
        // Connect to input system
        InputManager.Instance.PlayerControls.Player.Pause.performed += _ => TogglePause();

        // Hide the pause menu at the start
        PauseMenu.SetActive(false);
    }

    private void TogglePause()
    {
        if (_isPaused)
            Resume(PauseMenu.transform.GetChild(0).gameObject);
        else
            LoadPauseMenu();
    }


    public void OnPointerEnter(GameObject textObject)
    {
        TMP_Text tmpText = textObject.GetComponent<TMP_Text>();

        if (tmpText != null)
            tmpText.color = hoverColor;
    }

    /// <summary>
    /// Changes the text color of the TextMeshPro back to the normal color.
    /// Attach this to the PointerExit event.
    /// </summary>
    public void OnPointerExit(GameObject textObject)
    {
        TMP_Text tmpText = textObject.GetComponent<TMP_Text>();

        if (tmpText != null)
            tmpText.color = normalColor;
    }


    /// <summary>
    /// Changes the color of the TextMeshPro text to the click color.
    /// </summary>
    /// <param name="textObject">The GameObject associated with the button.</param>
    private void ChangeClickColor(GameObject textObject)
    {
        return;
        
        TMP_Text tmpText = textObject.GetComponent<TMP_Text>();

        if (tmpText != null)
            tmpText.color = clickColor;
    }


    private void FixPhysics()
    {
        Time.fixedDeltaTime = this.fixedDeltaTime * Time.timeScale;
    }

    /// <summary>
    /// Called when the Resume button is clicked.
    /// </summary>
    public void Resume(GameObject textObject)
    {
        ChangeClickColor(textObject);

        // Add your Resume logic here
        Debug.Log("Resume game");

        Time.timeScale = tempTimeScale;
        FixPhysics();

        //Hide menu
        PauseMenu.SetActive(false);

        //Set pause to false
        _isPaused = false;
    }

    private void LoadPauseMenu()
    {
        fixedDeltaTime = Time.fixedDeltaTime;

        // Un-hide the pause menu
        PauseMenu.SetActive(true);

        // Save the current timescale
        tempTimeScale = Time.timeScale;

        //Pause the game
        Time.timeScale = 0;
        FixPhysics();

        // Set pause to true
        _isPaused = true;

        //Make pause menu work with controller and keyboard
    }

    /// <summary>
    /// Called when the Journal button is clicked.
    /// </summary>
    public void Journal(GameObject textObject)
    {
        ChangeClickColor(textObject);

        // Add your Journal logic here
        Debug.Log("Open Journal");
    }


    /// <summary>
    /// Called when the Settings button is clicked.
    /// </summary>
    public void Settings(GameObject textObject)
    {
        ChangeClickColor(textObject);
        // Add your Settings logic here
        Debug.Log("Open Settings");
    }

    /// <summary>
    /// Called when the Exit button is clicked.
    /// </summary>
    public void Exit(GameObject textObject)
    {
        ChangeClickColor(textObject);
        // Add your Exit logic here
        Debug.Log("Exit the game");

        //Go back to main menu
        //SceneManager.LoadScene("MainMenu");
    }
}