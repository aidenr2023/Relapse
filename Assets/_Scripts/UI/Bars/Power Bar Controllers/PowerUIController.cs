using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PowerUIController : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private Image currentPowerImage;

    [SerializeField] private Image[] powerImages;

    #endregion

    #region Private Fields

    private Player _player;

    #endregion

    private void Start()
    {
        // Set the player
        _player = Player.Instance;
    }

    private void Update()
    {
        // Update the images
        UpdateImages();
    }

    private void UpdateImages()
    {
        // Get the power manager
        var powerManager = _player.PlayerPowerManager;

        // Get the current power
        var currentPower = powerManager.CurrentPower;

        if (currentPower != null)
        {
            // Get the power token
            var powerToken = powerManager.CurrentPowerToken;

            // Get the cooldown percentage
            var cooldownPercentage = powerToken.CooldownPercentage;

            // Set the current power image fill amount
            currentPowerImage.fillAmount = (powerToken.CooldownPercentage != 0)
                ? powerToken.CooldownPercentage
                : 1;

            // Set the sprite of the image
            currentPowerImage.sprite = currentPower.Icon;
        }

        var powers = powerManager.Powers.ToArray();

        // Set the rest of the power images
        for (var i = 0; i < powerImages.Length; i++)
        {
            // If there is no power at this index, set the fill amount to 1 and the sprite to null
            if (i >= powers.Length)
            {
                powerImages[i].fillAmount = 1;
                powerImages[i].sprite = null;
                continue;
            }

            var cPower = powers[i];

            // Get the power token
            var powerToken = powerManager.GetPowerToken(cPower);

            // If there is no power token, set the fill amount to 1
            if (powerToken == null)
                powerImages[i].fillAmount = 1;

            // Set the fill amount
            else
                powerImages[i].fillAmount = (powerToken.CooldownPercentage != 0)
                    ? powerToken.CooldownPercentage
                    : 1;

            // Set the sprite of the image
            powerImages[i].sprite = cPower.Icon;
        }
    }
}