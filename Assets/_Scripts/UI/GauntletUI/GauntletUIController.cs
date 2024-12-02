using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GauntletUIController : MonoBehaviour
{
    #region Serialized Fields

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

    #endregion

    #region Private Fields

    private CountdownTimer flashTimer = new CountdownTimer(0.5f, true);

    #endregion

    private void Awake()
    {
        // Assert that the player is not null
        Debug.Assert(player != null, "GauntletUIController: Player is null.");

        // Assert that the powerImage is not null
        Debug.Assert(powerImage != null, "GauntletUIController: Power Image is null.");

        // Set up the timer
        // Restart the timer
        flashTimer.OnTimerEnd += () => flashTimer.Reset();

        flashTimer.Reset();
        flashTimer.Start();
    }

    private void Update()
    {
        // Update the flash timer
        UpdateFlashTimer();

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

        var opacity = 1f;

        var powerToken = player.PlayerPowerManager.CurrentPowerToken;

        // If the power is done charging, flash the image
        if (powerToken != null)
        {
            // Create a sine wave from 0 to 1
            var value = Mathf.Sin(flashTimer.Percentage * Mathf.PI) / 2 + 0.5f;

            // If the power is done charging, flash the image
            if (powerToken.ChargePercentage >= 1)
                opacity = value;

            // If the power is currently active, flash the image
            else if (powerToken.IsActiveEffectOn)
                opacity = value;
        }

        // Set the color to white
        powerImage.color = new Color(1, 1, 1, opacity);

        // Set the image to the current power's icon
        powerImage.sprite = player.PlayerPowerManager.CurrentPower.Icon;

        // Set the fill amount to 1
        powerImage.fillAmount = 1;

        // If the current power token is not null,
        // set the fill amount to the cooldown percentage
        if (player.PlayerPowerManager.CurrentPowerToken != null)
        {
            // If the power is active, set the fill amount to 1 - the active effect percentage
            if (player.PlayerPowerManager.CurrentPowerToken.IsActiveEffectOn)
                powerImage.fillAmount = 1 - player.PlayerPowerManager.CurrentPowerToken.ActivePercentage;

            // Otherwise, set the fill amount to the cooldown percentage
            else
                powerImage.fillAmount = player.PlayerPowerManager.CurrentPowerToken.CooldownPercentage;
        }
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

    private void UpdateFlashTimer()
    {
        var timeChange = Time.deltaTime;

        // If the power is done with the cooldown,
        if (player.PlayerPowerManager.CurrentPowerToken != null)
        {
            // If the active effect is on, multiply the time change by 1
            if (player.PlayerPowerManager.CurrentPowerToken.IsActiveEffectOn)
                timeChange *= 1;

            // Otherwise, if the charge percentage is >= 1, multiply the time change by 4
            else if (player.PlayerPowerManager.CurrentPowerToken.CooldownPercentage >= 1)
                timeChange *= 4;
        }


        flashTimer.Update(timeChange);
    }
}