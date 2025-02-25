using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowerScroll : MonoBehaviour
{
    [SerializeField] private Image powerIcon; // Power icon UI element
    [SerializeField] private Slider chargeSlider; // Charge bar UI element
    [SerializeField] private Slider cooldownSlider; // Cooldown bar UI element
    [SerializeField] private Text powerNameText; // Power name UI text

    private void Update()
    {
        // Update the power UI
        UpdatePowerUI();
    }

    // Update the UI elements based on the current power
    private void UpdatePowerUI()
    {
        // Get the current power & power token
        var currentPower = Player.Instance.PlayerPowerManager.CurrentPower;
        var powerToken = Player.Instance.PlayerPowerManager.GetPowerToken(currentPower);

        // If the current power is null, hide the UI elements
        if (currentPower == null)
        {
            SetUIVisibility(false);
            return;
        }

        // Show the UI elements
        SetUIVisibility(true);

        // Update power icon, Assuming you have a sprite for each power
        if (powerIcon != null)
            powerIcon.sprite = currentPower.Icon;

        // Update power name
        if (powerNameText != null)
            powerNameText.text = currentPower.name;

        // Update the charge and cooldown sliders
        UpdateSlider(currentPower, powerToken);
    }

    private void UpdateSlider(PowerScriptableObject currentPower, PowerToken powerToken)
    {
        // If the cooldown and charge sliders are separate, update them separately
        if (chargeSlider != cooldownSlider)
        {
            // Update the charge slider
            if (chargeSlider != null)
                chargeSlider.value = 1 - powerToken.CooldownPercentage;

            // Update the cooldown slider
            if (cooldownSlider != null)
                cooldownSlider.value = powerToken.CooldownPercentage;
        }

        // If the cooldown and charge sliders are the same, update them together
        else
        {
            // If the current power is cooling down, update the slider based on the cooldown percentage
            if (powerToken.IsCoolingDown)
                cooldownSlider.value = 1 - powerToken.CooldownPercentage;

            // Otherwise, update the slider based on the charge percentage
            else
                chargeSlider.value = powerToken.ChargePercentage;
        }
    }

    private void SetUIVisibility(bool isVisible)
    {
        powerIcon?.gameObject.SetActive(isVisible);
        chargeSlider?.gameObject.SetActive(isVisible);
        cooldownSlider?.gameObject.SetActive(isVisible);
        powerNameText?.gameObject.SetActive(isVisible);
    }
}