using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PowerUIController : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image currentPowerImage;

    [Space, SerializeField] private CanvasGroup powerIconsCanvasGroup;
    [SerializeField] private PowerIconController[] powerImages;
    [SerializeField, Range(0, 1)] private float powerIconsMinOpacity = .75f;
    [SerializeField] private float powerIconsOpacityLerpAmount = .1f;
    [SerializeField] private float powerIconsStayOnScreenTime = .5f;
    [SerializeField] private TMP_Text powerNameText;
    [SerializeField] private Vector3 powerIconFadeInOffset;

    [Space, SerializeField] private Color selectedColor = Color.white;
    [SerializeField] private Color unselectedColor = Color.black;

    [Space, SerializeField] private TMP_FontAsset font;

    #endregion

    #region Private Fields

    private Player _player;

    private CountdownTimer _powerIconsStayOnScreenTimer;

    private int _previousPowerIndex;

    private Vector3 _currentPowerIconsOffset;

    private bool _isFadingIn;

    #endregion

    private void Awake()
    {
        // Create the power icons stay on screen timer
        _powerIconsStayOnScreenTimer = new CountdownTimer(powerIconsStayOnScreenTime);
        _powerIconsStayOnScreenTimer.Start();

        // Set the opacity of the power icons canvas group to 0
        powerIconsCanvasGroup.alpha = powerIconsMinOpacity;
    }

    private void Start()
    {
        // Set the player
        _player = Player.Instance;
    }

    private void Update()
    {
        // Update the images
        UpdateImages();

        // Update the power icons opacity
        UpdatePowerIconsOpacity();

        // Update the power icons offset
        UpdatePowerIconsOffset();

        // Update the power icon size
        UpdatePowerIconSize();
        
        // Update the power name text
        UpdatePowerNameText();
    }

    private void UpdateImages()
    {
        // Get the power manager
        var powerManager = _player.PlayerPowerManager;

        // Get the powers
        var powers = powerManager.Powers.ToArray();

        // If there are no powers, hide the canvas group
        if (powers.Length == 0)
            canvasGroup.alpha = powerIconsMinOpacity;

        // Show the canvas group
        canvasGroup.alpha = 1;

        // Get the current power
        var currentPower = powerManager.CurrentPower;

        if (currentPower != null)
        {
            currentPowerImage.enabled = true;

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
        else
            currentPowerImage.enabled = false;

        // Set the rest of the power images
        for (var i = 0; i < powerImages.Length; i++)
        {
            // If there is no power at this index, set the fill amount to 1 and the sprite to null
            if (i >= powers.Length)
            {
                // powerImages[i].fillAmount = 1;
                powerImages[i].SetFill(1);
                powerImages[i].SetForeground(null);
                powerImages[i].SetVisible(false);
                continue;
            }

            powerImages[i].SetVisible(true);

            var cPower = powers[i];

            // Set the color of the image
            if (currentPower != null && cPower == currentPower)
                powerImages[i].SetColor(selectedColor);
            else
                powerImages[i].SetColor(unselectedColor);

            // Get the power token
            var powerToken = powerManager.GetPowerToken(cPower);

            // If there is no power token, set the fill amount to 1
            if (powerToken == null)
                powerImages[i].SetFill(1);

            // Set the fill amount
            else
                powerImages[i].SetFill((powerToken.CooldownPercentage != 0)
                    ? powerToken.CooldownPercentage
                    : 1);

            // Set the sprite of the image
            powerImages[i].SetForeground(cPower.Icon);
        }
    }

    private void UpdatePowerIconsOpacity()
    {
        // Update the stay on screen timer
        _powerIconsStayOnScreenTimer?.SetMaxTime(powerIconsStayOnScreenTime);
        _powerIconsStayOnScreenTimer?.Update(Time.unscaledDeltaTime);

        // Get the current power index
        var currentPowerIndex = _player.PlayerPowerManager.CurrentPowerIndex;

        // If the current power index is different from the previous power index, reset the stay on screen timer
        if (currentPowerIndex != _previousPowerIndex)
            _powerIconsStayOnScreenTimer?.Reset();

        var desiredOpacity = 1f;

        _isFadingIn = true;

        // If the stay on screen timer is complete, set the desired opacity to 0
        if (_powerIconsStayOnScreenTimer?.IsComplete ?? false)
        {
            desiredOpacity = powerIconsMinOpacity;
            _isFadingIn = false;
        }

        const float defaultFrameTime = 1 / 60f;
        var frameAmount = Time.unscaledDeltaTime / defaultFrameTime;

        // Set the opacity of the power icons canvas group
        powerIconsCanvasGroup.alpha = Mathf.Lerp(
            powerIconsCanvasGroup.alpha,
            desiredOpacity,
            powerIconsOpacityLerpAmount * frameAmount
        );

        // Set the previous power index
        _previousPowerIndex = currentPowerIndex;
    }

    private void UpdatePowerIconsOffset()
    {
        var previousPowerIconsOffset = _currentPowerIconsOffset;

        // var direction = _isFadingIn ? 1 : -1;
        var direction = 1;

        _currentPowerIconsOffset = Vector3.Lerp(
            powerIconFadeInOffset * direction,
            Vector3.zero,
            powerIconsCanvasGroup.alpha
        );

        // Remove the previous offset from the icon group's position
        powerIconsCanvasGroup.transform.localPosition -= previousPowerIconsOffset;

        // Add the current offset to the icon group's position
        powerIconsCanvasGroup.transform.localPosition += _currentPowerIconsOffset;
    }

    private void UpdatePowerIconSize()
    {
        // Get the current power
        var currentPower = _player.PlayerPowerManager.CurrentPower;

        // Return if the current power is null
        if (currentPower == null)
            return;

        // Get the power token
        var powerToken = _player.PlayerPowerManager.CurrentPowerToken;

        // Return if the power token is null
        if (powerToken == null)
            return;

        // Set the scale of the current power image
        for (var i = 0; i < powerImages.Length; i++)
        {
            var targetScale = 1f;

            if (i == _player.PlayerPowerManager.CurrentPowerIndex)
                targetScale = 1.5f;

            // Lerp the scale of the power image
            powerImages[i].LerpScale(targetScale, powerIconsOpacityLerpAmount);
        }
    }

    private void UpdatePowerNameText()
    {
        // Return if the power name text is null
        if (powerNameText == null)
            return;

        // Get the current power
        var currentPower = _player.PlayerPowerManager.CurrentPower;

        // Return if the current power is null
        if (currentPower == null)
            return;

        // Set the text of the power name text
        powerNameText.text = currentPower.PowerName;

        // Set the opacity of the power name text
        powerNameText.alpha = powerIconsCanvasGroup.alpha;

        // Set the font of the power name text
        powerNameText.font = font;
    }
}