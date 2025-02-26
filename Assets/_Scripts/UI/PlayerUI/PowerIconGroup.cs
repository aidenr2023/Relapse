using System;
using System.Linq;
using TMPro;
using UnityEngine;

public class PowerIconGroup : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private PowerIconController[] powerImages;
    [SerializeField] private TMP_Text powerNameText;

    [SerializeField] private float powerIconsStayOnScreenTime = 1f;
    [SerializeField, Range(0, 1)] private float powerIconsMinOpacity = .75f;
    [SerializeField] private Vector3 powerIconFadeInOffset;

    [Space, SerializeField] private TMP_FontAsset font;

    [SerializeField] private Color disabledColor = Color.HSVToRGB(0, 0, .25f);

    #endregion

    #region Private Fields

    private CountdownTimer _powerIconsStayOnScreenTimer;

    private int _previousPowerIndex;

    private Vector3 _currentPowerIconsOffset;

    private bool _isFadingIn;

    #endregion

    #region Getters

    public PowerIconController[] PowerImages => powerImages;

    #endregion

    private void Awake()
    {
        // Create the power icons stay on screen timer
        _powerIconsStayOnScreenTimer = new CountdownTimer(powerIconsStayOnScreenTime);
        _powerIconsStayOnScreenTimer.Start();

        // Set the opacity of the power icons canvas group to 0
        canvasGroup.alpha = powerIconsMinOpacity;
    }

    public void UpdateIcons(PowerUIController controller)
    {
        // Return if the object is disabled
        if (!gameObject.activeInHierarchy)
            return;
        
        // Update the stay on screen timer
        _powerIconsStayOnScreenTimer.SetMaxTime(powerIconsStayOnScreenTime);

        if (PauseMenuManager.Instance.IsPaused)
            return;

        _powerIconsStayOnScreenTimer.Update(Time.unscaledDeltaTime);

        // Update the images
        UpdateImages(controller);

        // Update the power icons opacity
        UpdatePowerIconsOpacity(controller);

        // Update the power icons offset
        UpdatePowerIconsOffset(controller);

        // Update the power icon size
        UpdatePowerIconSize(controller);

        // Update the power name text
        UpdatePowerNameText();
    }

    private void UpdateImages(PowerUIController controller)
    {
        // Get the power manager
        var powerManager = Player.Instance.PlayerPowerManager;

        // Get the powers
        var powers = powerManager.Powers.ToArray();

        // If there are no powers, hide the canvas group
        if (powers.Length == 0)
            canvasGroup.alpha = powerIconsMinOpacity;

        // // Show the canvas group
        // canvasGroup.alpha = 1;

        // Get the current power
        var currentPower = powerManager.CurrentPower;

        // Set the rest of the power images
        for (var i = 0; i < powerImages.Length; i++)
        {
            // If there is no power at this index, set the fill amount to 1 and the sprite to null
            if (i >= powers.Length)
            {
                powerImages[i].SetFill(1);
                powerImages[i].SetForeground(null);
                powerImages[i].SetVisible(false);
                continue;
            }

            powerImages[i].SetVisible(true);

            var cPower = powers[i];

            // Set the color of the image
            if (currentPower != null && cPower == currentPower)
                powerImages[i].SetBgColor(controller.SelectedColor);
            else
                powerImages[i].SetBgColor(controller.UnselectedColor);

            // Get the power token
            var powerToken = powerManager.GetPowerToken(cPower);

            // If there is no power token, set the fill amount to 1
            if (powerToken == null)
                powerImages[i].SetFill(1);

            // Set the fill amount
            else
            {
                powerImages[i].SetFill((powerToken.CooldownPercentage != 0)
                    ? powerToken.CooldownPercentage
                    : 1);
                
                // If the power is currently cooling down, set the color to the disabled color
                if (powerToken.IsCoolingDown)
                {
                    powerImages[i].SetFgColor(disabledColor);
                    powerImages[i].SetBgOpacity(1f);
                    
                }
                else
                {
                    powerImages[i].SetFgColor(Color.white);
                    powerImages[i].SetBgOpacity(0f);
                }
            }

            // Set the sprite of the image
            powerImages[i].SetForeground(cPower.Icon);

            // var angle = cPower.PowerType == PowerType.Drug
            //     ? drugRotation
            //     : medRotation;

            // if (powerToken != null)
            // {
            //     var angle = powerToken.CooldownPercentage < 1
            //         ? medRotation
            //         : drugRotation;
            //
            //     // Set the power of the image
            //     powerImages[i].SetRotation(angle);
            // }
        }
    }

    private void UpdatePowerIconsOpacity(PowerUIController controller)
    {
        // Get the current power index
        var currentPowerIndex = Player.Instance.PlayerPowerManager.CurrentPowerIndex;

        // If the current power index is different from the previous power index, reset the stay on screen timer
        if (currentPowerIndex != _previousPowerIndex)
        {
            _powerIconsStayOnScreenTimer.Reset();
            _isFadingIn = true;
        }

        var desiredOpacity = 1f;

        // If the stay on screen timer is complete, set the desired opacity to min opacity
        if (_powerIconsStayOnScreenTimer.IsComplete)
        {
            desiredOpacity = powerIconsMinOpacity;
            _isFadingIn = false;
        }

        // Set the opacity of the power icons canvas group
        canvasGroup.alpha = Mathf.Lerp(
            canvasGroup.alpha,
            desiredOpacity,
            CustomFunctions.FrameAmount(controller.PowerIconsOpacityLerpAmount, false, true)
        );

        // Set the previous power index
        _previousPowerIndex = currentPowerIndex;
    }

    private void UpdatePowerIconsOffset(PowerUIController controller)
    {
        var previousOffset = _currentPowerIconsOffset;

        var targetValue = Vector3.zero;

        if (!_isFadingIn)
            targetValue = powerIconFadeInOffset;

        _currentPowerIconsOffset = Vector3.Lerp(
            _currentPowerIconsOffset,
            targetValue,
            CustomFunctions.FrameAmount(controller.PowerIconsOpacityLerpAmount, false, true)
        );

        // Remove the previous offset from the power icons canvas group
        canvasGroup.transform.localPosition -= previousOffset;

        // Set the position of the power icons canvas group
        canvasGroup.transform.localPosition += _currentPowerIconsOffset;
    }

    private void UpdatePowerIconSize(PowerUIController controller)
    {
        // Get the current power
        var currentPower = Player.Instance.PlayerPowerManager.CurrentPower;

        // Return if the current power is null
        if (currentPower == null)
            return;

        // Get the power token
        var powerToken = Player.Instance.PlayerPowerManager.CurrentPowerToken;

        // Return if the power token is null
        if (powerToken == null)
            return;

        // Set the scale of the current power image
        for (var i = 0; i < powerImages.Length; i++)
        {
            var targetScale = 1f;

            if (i == Player.Instance.PlayerPowerManager.CurrentPowerIndex)
                targetScale = 1.5f;

            // Lerp the scale of the power image
            powerImages[i].LerpScale(targetScale, controller.PowerIconsOpacityLerpAmount);
        }
    }

    private void UpdatePowerNameText()
    {
        // Return if the power name text is null
        if (powerNameText == null)
            return;

        // Get the current power
        var currentPower = Player.Instance.PlayerPowerManager.CurrentPower;

        // Return if the current power is null
        if (currentPower == null)
            powerNameText.text = "";
        // Set the text of the power name text
        else
            powerNameText.text = currentPower.PowerName;

        // Set the opacity of the power name text
        powerNameText.alpha = canvasGroup.alpha;

        // Set the font of the power name text
        powerNameText.font = font;
    }
}