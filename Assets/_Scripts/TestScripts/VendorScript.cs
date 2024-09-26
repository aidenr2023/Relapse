using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class VendorScript : MonoBehaviour, IInteractable
{
    [SerializeField] private PowerScriptableObject availablePower;

    [SerializeField] private Image powerImage;
    [SerializeField] private TMP_Text powerNameText;

    // TODO: In the future, find a better way to get the player that is looking at the vendor
    private TestPlayer _testPlayer;

    /// <summary>
    /// False - The player does not have the power & needs to pick it up.
    /// True - The player has the power & needs to drop it.
    /// </summary>
    private bool _removalMode;

    #region IInteractable

    public GameObject GameObject => gameObject;

    public string InteractText => _removalMode ? $"Remove {availablePower.name}" : $"Pick Up {availablePower.name}";
    public bool IsCurrentlySelected { get; set; }
    public bool IsInteractable => true;

    #endregion

    private void Awake()
    {
        // Get the player component
        _testPlayer = FindObjectOfType<TestPlayer>();
    }

    private void Update()
    {
        // Update the power UI
        UpdatePowerUI();

        // Update the removal mode
        UpdateRemovalMode(_testPlayer?.PlayerPowerManager);
    }

    private void UpdatePowerUI()
    {
        // If the current power is null, hide the UI elements
        // Hide the UI elements if the interactable is not interactable
        // Hide the UI elements if the interactable is not selected
        if (availablePower == null)
        {
            SetUIVisibility(false);
            return;
        }

        // Show the UI elements
        SetUIVisibility(true);

        // Update power icon
        if (powerImage != null)
            powerImage.sprite = availablePower.Icon;

        // Update power name
        if (powerNameText != null)
            powerNameText.text = availablePower.name;
    }

    /// <param name="playerPowerManager"></param>
    /// <returns></returns>
    private void UpdateRemovalMode(TestPlayerPowerManager playerPowerManager)
    {
        if (playerPowerManager == null)
            return;
        
        // If the player has the power, set removal mode to true
        _removalMode = playerPowerManager.HasPower(availablePower);
    }


    private void SetUIVisibility(bool isVisible)
    {
        powerImage?.gameObject.SetActive(isVisible);
        powerNameText?.gameObject.SetActive(isVisible);
    }

    public void Interact(PlayerInteraction playerInteraction)
    {
        // Add the power to the player's inventory
        if (_removalMode)
            playerInteraction.Player.PlayerPowerManager.RemovePower(availablePower);
        
        // Remove the power from the player's inventory
        else
            playerInteraction.Player.PlayerPowerManager.AddPower(availablePower);
    }
}