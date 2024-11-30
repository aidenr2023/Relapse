using System;
using UnityEngine;
using UnityEngine.UI;

public class GauntletUIController : MonoBehaviour
{
    [SerializeField] private Player player;

    [SerializeField] private Image powerImage;

    private void Awake()
    {
        // Assert that the player is not null
        Debug.Assert(player != null, "GauntletUIController: Player is null.");

        // Assert that the powerImage is not null
        Debug.Assert(powerImage != null, "GauntletUIController: Power Image is null.");
    }

    private void Update()
    {
        // Update the power image
        UpdatePowerImage();
    }

    private void UpdatePowerImage()
    {
        // If the current power is null, set the sprite to null and return
        if (player.PlayerPowerManager.CurrentPower == null)
        {
            powerImage.sprite = null;

            // Set the color to transparent
            powerImage.color = new Color(0, 0, 0, 0);

            return;
        }

        // Set the color to white
        powerImage.color = Color.white;

        // Set the image to the current power's icon
        powerImage.sprite = player.PlayerPowerManager.CurrentPower.Icon;

        // Set the fill amount to 1
        powerImage.fillAmount = 1;

        Debug.Log($"Current Token: _{player.PlayerPowerManager.CurrentPowerToken}_");

        // If the current power token is not null, set the fill amount to the cooldown percentage
        if (player.PlayerPowerManager.CurrentPowerToken != null)
            powerImage.fillAmount = player.PlayerPowerManager.CurrentPowerToken.CooldownPercentage;
    }
}