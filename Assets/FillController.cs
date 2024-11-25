using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class FillController : MonoBehaviour
{
    // --- Fill Amount Control ---

    // Reference to the Image component
    private Image image;

    // Speed at which the fill amount changes
    [SerializeField]
    private float fillSpeed = 0.5f;

    // --- GameObject Switching ---

    // Array of GameObjects to switch between
    [SerializeField]
    private GameObject[] gameObjects;

    // Current active GameObject index
    private int currentIndex = 0;

    // Initialization
    void Start()
    {
        // --- Initialize Fill Amount ---

        // Get the Image component attached to the same GameObject
        image = GetComponent<Image>();

        if (image == null)
        {
            Debug.LogError("FillController: No Image component found on this GameObject.");
        }
        else
        {
            // Set the initial fill amount to 0
            image.fillAmount = 0f;
        }

        // --- Initialize GameObject Switching ---

        if (gameObjects == null || gameObjects.Length == 0)
        {
            Debug.LogError("FillController: No GameObjects assigned for switching.");
            return;
        }

        // Ensure that only the first GameObject is active at start
        for (int i = 0; i < gameObjects.Length; i++)
        {
            if (gameObjects[i] != null)
            {
                gameObjects[i].SetActive(i == currentIndex);
            }
            else
            {
                Debug.LogWarning($"FillController: GameObject at index {i} is not assigned.");
            }
        }
    }

    // Update is called once per frame
    void Update()
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
        if (image == null)
            return;

        // Check if the Up Arrow key is being held down
        if (Input.GetKey(KeyCode.UpArrow))
        {
            // Increase the fill amount
            image.fillAmount += fillSpeed * Time.deltaTime;

            // Clamp the fill amount to a maximum of 1
            image.fillAmount = Mathf.Clamp(image.fillAmount, 0f, 1f);
        }
        // Check if the Down Arrow key is being held down
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            // Decrease the fill amount
            image.fillAmount -= fillSpeed * Time.deltaTime;

            // Clamp the fill amount to a minimum of 0
            image.fillAmount = Mathf.Clamp(image.fillAmount, 0f, 1f);
        }
    }

    /// <summary>
    /// Handles switching between GameObjects based on Right and Left arrow key presses.
    /// </summary>
    private void HandleGameObjectSwitching()
    {
        // Right Arrow Key Pressed
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            SwitchToNextGameObject();
        }

        // Left Arrow Key Pressed
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            SwitchToPreviousGameObject();
        }
    }

    /// <summary>
    /// Switches to the next GameObject in the array.
    /// </summary>
    private void SwitchToNextGameObject()
    {
        if (currentIndex < gameObjects.Length - 1)
        {
            // Disable current GameObject
            if (gameObjects[currentIndex] != null)
            {
                gameObjects[currentIndex].SetActive(false);
            }

            // Increment index
            currentIndex++;

            // Enable next GameObject
            if (gameObjects[currentIndex] != null)
            {
                gameObjects[currentIndex].SetActive(true);
            }
            else
            {
                Debug.LogWarning($"FillController: GameObject at index {currentIndex} is not assigned.");
            }
        }
        else
        {
            Debug.Log("FillController: Already at the last GameObject. Cannot move to next.");
        }
    }

    /// <summary>
    /// Switches to the previous GameObject in the array.
    /// </summary>
    private void SwitchToPreviousGameObject()
    {
        if (currentIndex > 0)
        {
            // Disable current GameObject
            if (gameObjects[currentIndex] != null)
            {
                gameObjects[currentIndex].SetActive(false);
            }

            // Decrement index
            currentIndex--;

            // Enable previous GameObject
            if (gameObjects[currentIndex] != null)
            {
                gameObjects[currentIndex].SetActive(true);
            }
            else
            {
                Debug.LogWarning($"FillController: GameObject at index {currentIndex} is not assigned.");
            }
        }
        else
        {
            Debug.Log("FillController: Already at the first GameObject. Cannot move to previous.");
        }
    }
}
