using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RelapseScreen : GameMenu
{
    public static RelapseScreen Instance { get; private set; }

    [SerializeField] private Button firstSelectedButton;

    private void Awake()
    {
        // Set the instance to this
        Instance = this;
    }

    private void Start()
    {
        // Disable the game object
        gameObject.SetActive(false);
    }

    protected override void CustomOnEnable()
    {
        // Set the event system's current selected game object to the first selected game object
        EventSystem.current.SetSelectedGameObject(firstSelectedButton.gameObject);
    }

    protected override void CustomOnDisable()
    {
    }

    public void LoadScene(string sceneName)
    {
        // Set the timescale to 1
        Time.timeScale = 1;

        // Load the scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    public void Activate()
    {
        // Set the timescale to 0
        Time.timeScale = 0;

        // Enable the game object
        gameObject.SetActive(true);
    }
}