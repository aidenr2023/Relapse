using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GauntletUIController : MonoBehaviour
{
    [SerializeField] private Player player;

    [SerializeField] private Image powerImage;

    [SerializeField] private Image[] otherPowerImages;

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

        // Update the other power images
        UpdateOtherPowerImages();
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

        // If the current power token is not null, set the fill amount to the cooldown percentage
        if (player.PlayerPowerManager.CurrentPowerToken != null)
            powerImage.fillAmount = player.PlayerPowerManager.CurrentPowerToken.CooldownPercentage;
    }

    private void UpdateOtherPowerImages()
    {
        var playerPowers = player.PlayerPowerManager.Powers.ToArray();

        var otherPowerImageIndex = 0;
        var otherPowerCount = otherPowerImages.Length;

        // Return if there are <= 1, return
        if (playerPowers.Length <= 1)
        {
            foreach (var otherPowerImage in otherPowerImages)
            {
                otherPowerImage.sprite = null;
                otherPowerImage.color = new Color(0, 0, 0, 0);
            }

            return;
        }

        // Loop through the player powers
        foreach (var cPower in playerPowers)
        {
            // If the power is the current power, continue
            if (cPower == player.PlayerPowerManager.CurrentPower)
                continue;

            // If the other power image index is greater than the other power count, break
            if (otherPowerImageIndex >= otherPowerCount)
                break;

            // Set the other power image sprite to the power's icon
            otherPowerImages[otherPowerImageIndex].sprite = cPower.Icon;

            // Set the other power image color to white
            otherPowerImages[otherPowerImageIndex].color = Color.white;

            // Increment the other power image index
            otherPowerImageIndex++;
        }
    }
}