using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class FillController : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private Player player;

    // Speed at which the fill amount changes
    [SerializeField] private float fillSpeed = 0.5f;

    // Array of GameObjects to switch between
    [SerializeField] private GameObject[] gameObjects;

    #endregion

    #region Private Fields

    // Reference to the Image component
    private Image _image;

    // Current active GameObject index
    private int _currentIndex;

    #endregion

    private void Awake()
    {
        // Get the Image component attached to the same GameObject
        _image = GetComponent<Image>();

        // Assert that the image is not null
        Debug.Assert(_image != null, "FillController: Image component not found on this GameObject.");


        // Assert that the player is not null
        Debug.Assert(player != null, "FillController: Player is null.");
    }

    // Initialization
    private void Start()
    {
        // Set the fill amount to the player's toxicity level
        SetFillAmount(player.PlayerInfo.ToxicityPercentage);

        // --- Initialize GameObject Switching ---
        if (gameObjects == null || gameObjects.Length == 0)
        {
            Debug.LogError("FillController: No GameObjects assigned for switching.");
            return;
        }

        // Ensure that only the first GameObject is active at start
        for (var i = 0; i < gameObjects.Length; i++)
        {
            if (gameObjects[i] != null)
                gameObjects[i].SetActive(i == _currentIndex);
            else
                Debug.LogWarning($"FillController: GameObject at index {i} is not assigned.");
        }
    }

    // Update is called once per frame
    private void Update()
    {
        // --- Handle Fill Amount Changes ---
        HandleFillAmount();

        // --- Handle GameObject Switching ---
        HandleGameObjectSwitching();
    }

    /// <summary>
    /// Handles the fill amount based on Up and Down arrow keys.
    /// </summary>
    private void HandleFillAmount()
    {
        if (_image == null)
            return;

        // If the player is null, return
        if (player == null)
            return;

        // Set the fill amount to the player's toxicity level
        SetFillAmount(player.PlayerInfo.ToxicityPercentage);

        // // Check if the Up Arrow key is being held down
        // if (Input.GetKey(KeyCode.UpArrow))
        //     SetFillAmount(_image.fillAmount + fillSpeed * Time.deltaTime);
        //
        // // Check if the Down Arrow key is being held down
        // else if (Input.GetKey(KeyCode.DownArrow))
        //     SetFillAmount(_image.fillAmount - fillSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Handles switching between GameObjects based on Right and Left arrow key presses.
    /// </summary>
    private void HandleGameObjectSwitching()
    {
        // // Right Arrow Key Pressed
        // if (Input.GetKeyDown(KeyCode.RightArrow))
        //     SwitchToNextGameObject();
        //
        // // Left Arrow Key Pressed
        // if (Input.GetKeyDown(KeyCode.LeftArrow))
        //     SwitchToPreviousGameObject();


    }

    /// <summary>
    /// Switches to the next GameObject in the array.
    /// </summary>
    private void SwitchToNextGameObject()
    {
        if (_currentIndex < gameObjects.Length - 1)
        {
            // Disable current GameObject
            if (gameObjects[_currentIndex] != null)
                gameObjects[_currentIndex].SetActive(false);

            // Increment index
            _currentIndex++;

            // Enable next GameObject
            if (gameObjects[_currentIndex] != null)
                gameObjects[_currentIndex].SetActive(true);
            else
                Debug.LogWarning($"FillController: GameObject at index {_currentIndex} is not assigned.");
        }
        else
            Debug.Log("FillController: Already at the last GameObject. Cannot move to next.");
    }

    /// <summary>
    /// Switches to the previous GameObject in the array.
    /// </summary>
    private void SwitchToPreviousGameObject()
    {
        if (_currentIndex > 0)
        {
            // Disable current GameObject
            if (gameObjects[_currentIndex] != null)
                gameObjects[_currentIndex].SetActive(false);

            // Decrement index
            _currentIndex--;

            // Enable previous GameObject
            if (gameObjects[_currentIndex] != null)
                gameObjects[_currentIndex].SetActive(true);
            else
                Debug.LogWarning($"FillController: GameObject at index {_currentIndex} is not assigned.");
        }
        else
            Debug.Log("FillController: Already at the first GameObject. Cannot move to previous.");
    }

    public void SetFillAmount(float fillAmount)
    {
        if (_image == null)
            return;

        _image.fillAmount = Mathf.Clamp(fillAmount, 0f, 1f);
    }
}