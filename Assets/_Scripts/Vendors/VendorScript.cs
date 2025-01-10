using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class VendorScript : MonoBehaviour, IInteractable
{
    private enum VendorInteractionMode
    {
        PickUp,
        Upgrade,
        Remove
    }

    [SerializeField] private PowerScriptableObject availablePower;

    [SerializeField] private Image powerImage;
    [SerializeField] private TMP_Text powerNameText;

    /// <summary>
    /// A variable to determine the interaction mode of the vendor.
    /// </summary>
    private VendorInteractionMode _interactionMode;

    #region IInteractable

    public GameObject GameObject => gameObject;

    public bool IsCurrentlySelected { get; set; }
    public bool IsInteractable => true;

    public bool HasOutline { get; set; }

    public HashSet<Material> OutlineMaterials { get; } = new();

    public InteractionIcon InteractionIcon => InteractionIcon.Action;

    #endregion

    private void Awake()
    {
    }

    private void Update()
    {
        // Update the power UI
        UpdatePowerUI();
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
    private void UpdateRemovalMode(PlayerPowerManager playerPowerManager)
    {
        if (playerPowerManager == null)
            return;

        var playerPowerToken = playerPowerManager.GetPowerToken(availablePower);

        if (playerPowerManager.HasPower(availablePower))
        {
            // Check to see if the current power has any upgrades available
            if (playerPowerToken.CurrentLevel < availablePower.MaxLevel)
                _interactionMode = VendorInteractionMode.Upgrade;

            // If not, set the interaction mode to remove
            else
                _interactionMode = VendorInteractionMode.Remove;
        }

        // Set the interaction mode to pick up
        else
            _interactionMode = VendorInteractionMode.PickUp;
    }


    private void SetUIVisibility(bool isVisible)
    {
        powerImage?.gameObject.SetActive(isVisible);
        powerNameText?.gameObject.SetActive(isVisible);
    }

    public void Interact(PlayerInteraction playerInteraction)
    {
        // Get the power manager from the player
        var playerPowerManager = playerInteraction.Player.PlayerPowerManager;

        switch (_interactionMode)
        {
            case VendorInteractionMode.PickUp:
                playerPowerManager.AddPower(availablePower);
                break;

            case VendorInteractionMode.Upgrade:
                // Get the power token from the player
                var pToken = playerPowerManager.GetPowerToken(availablePower);

                // Get the new level of the power
                var newLevel = pToken.CurrentLevel + 1;

                // Update the power level
                playerPowerManager.SetPowerLevel(availablePower, newLevel);
                break;

            case VendorInteractionMode.Remove:
                playerPowerManager.RemovePower(availablePower);
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void LookAtUpdate(PlayerInteraction playerInteraction)
    {
        // Update the removal mode
        UpdateRemovalMode(playerInteraction.Player.PlayerPowerManager);
    }

    public string InteractText(PlayerInteraction playerInteraction)
    {
        switch (_interactionMode)
        {
            case VendorInteractionMode.PickUp:
                return $"Pick Up {availablePower.PowerName}";

            case VendorInteractionMode.Upgrade:
                // Get the power level of the player
                var powerLevel = playerInteraction.Player.PlayerPowerManager.GetPowerToken(availablePower).CurrentLevel;

                // Get the 1-indexed power level
                powerLevel++;

                return $"Upgrade {availablePower.PowerName} to level {powerLevel + 1}";

            case VendorInteractionMode.Remove:
                return $"Remove {availablePower.PowerName}";

            default:
                return $"\"{_interactionMode}\" NOT HANDLED";
        }
    }
}