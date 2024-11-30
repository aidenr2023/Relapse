using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GauntletUIController : MonoBehaviour
{
    [SerializeField] private Player player;

    [SerializeField] private Image powerImage;

    [SerializeField] private Image[] otherPowerImages;

    [SerializeField] private GameObject fireballEquipped;
    [SerializeField] private GameObject regenEquipped;
    [SerializeField] private GameObject damageMultEquipped;

    [SerializeField] private PowerScriptableObject fireballPower;
    [SerializeField] private PowerScriptableObject regenPower;
    [SerializeField] private PowerScriptableObject damageMultPower;

    [SerializeField] private TMP_Text relapseCounter;

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

        // Update the game object thing
        UpdateGameObjectThing();

        // Update the relapse counter
        UpdateRelapseCounter();
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

    private void UpdateGameObjectThing()
    {
        // Get the current power
        var currentPower = player.PlayerPowerManager.CurrentPower;

        // Set all equipped game objects to false
        fireballEquipped.SetActive(false);
        regenEquipped.SetActive(false);
        damageMultEquipped.SetActive(false);
        // otherEquipped.SetActive(false);

        // If the current power is null, return
        if (currentPower == null)
            return;

        // If the current power is the fireball power, set the fireball equipped game object to true
        if (currentPower == fireballPower)
            fireballEquipped.SetActive(true);

        // If the current power is the regen power, set the regen equipped game object to true
        else if (currentPower == regenPower)
            regenEquipped.SetActive(true);

        // If the current power is the damage mult power, set the damage mult equipped game object to true
        else if (currentPower == damageMultPower)
            damageMultEquipped.SetActive(true);

        // // If the current power is not any of the above, set the other equipped game object to true
        // else
        //     otherEquipped.SetActive(true);
    }

    private void UpdateRelapseCounter()
    {
        // If the relapse counter is null, return
        if (relapseCounter == null)
            return;

        // Set the relapse counter text to the player's relapse count
        relapseCounter.text = $"x{player.PlayerInfo.RelapseCount}";
    }
}