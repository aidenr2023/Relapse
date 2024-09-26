using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VendorScript : MonoBehaviour, IInteractable
{
    [SerializeField] private PowerScriptableObject availablePower;

    [SerializeField] private Image powerImage;
    [SerializeField] private TMP_Text powerNameText;

    #region IInteractable

    public GameObject GameObject => gameObject;

    public string InteractText => "";
    public bool IsCurrentlySelected { get; set; }
    public bool IsInteractable => true;

    #endregion

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

    private void SetUIVisibility(bool isVisible)
    {
        powerImage?.gameObject.SetActive(isVisible);
        powerNameText?.gameObject.SetActive(isVisible);
    }

    public void Interact(PlayerInteraction playerInteraction)
    {
        // Add the power at the selected index to the player's inventory
        playerInteraction.Player.PlayerPowerManager.AddPower(availablePower);
    }
}