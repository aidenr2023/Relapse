using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowerScroll : MonoBehaviour
{
    public Image powerIcon; // Power icon UI element
    public Slider chargeSlider; // Charge bar UI element
    public Slider cooldownSlider; // Cooldown bar UI element
    public Text powerNameText; // Power name UI text

    // Update the UI elements based on the current power
    public void UpdatePowerUI(PowerScriptableObject currentPower, PowerToken powerToken)
    {
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

        // Update the charge slider
        if (chargeSlider != null)
            chargeSlider.value = powerToken.ChargePercentage;

        // Update the cooldown slider
        if (cooldownSlider != null)
            cooldownSlider.value = powerToken.CooldownPercentage;

        // Debug.Log("Updating Power UI for: " + currentPower.name);
    }
    
    private void SetUIVisibility(bool isVisible)
    {
        powerIcon?.gameObject.SetActive(isVisible);
        chargeSlider?.gameObject.SetActive(isVisible);
        cooldownSlider?.gameObject.SetActive(isVisible);
        powerNameText?.gameObject.SetActive(isVisible);
    }
}