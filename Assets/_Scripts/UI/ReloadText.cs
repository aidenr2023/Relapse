using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class ReloadText : MonoBehaviour
{
    private const float LERP_THRESHOLD = 0.0001f;

    #region Serialized Fields

    [SerializeField] private CanvasGroup canvasGroup;

    [SerializeField, Range(0, 1)] private float maxOpacity = 1;
    [SerializeField, Range(0, 1)] private float opacityLerpAmount = 0.15f;
    [SerializeField, Range(0, 1)] private float positionLerpAmount = 0.15f;

    [SerializeField] private Vector3 offset;

    [SerializeField] private float floatingBobAmount = 0.1f;
    [SerializeField, Min(0)] private float floatingBobFrequency = 1;

    [SerializeField] private TMP_Text text;
    [SerializeField, Range(0, 1)] private float lowAmmoPercentage = 0.25f;
    [SerializeField] private Gradient lowAmmoGradient;
    [SerializeField] private float lowAmmoBlinkFrequency = 1;
    [SerializeField] private float lowAmmoGradientStop = 0.1f;
    [SerializeField] private Color reloadColor = Color.red;

    [SerializeField] private string lowAmmoText = "LOW AMMO!";
    [SerializeField] private string reloadText = "RELOAD!";

    [SerializeField] private GameObject buttonsParent;
    [SerializeField] private GameObject gamepadButtons;
    [SerializeField] private GameObject keyboardButtons;

    #endregion

    #region Private Fields

    private WeaponManager _weaponManager;
    private float _desiredOpacity;

    #endregion

    public Color Color => text.color;

    private void Awake()
    {
        // Set the initial desired opacity to 0
        _desiredOpacity = 0;

        // Set the canvas group's alpha to 0
        canvasGroup.alpha = 0;
    }

    private void Update()
    {
        // Update the information of the current weapon manager
        UpdateInformation();

        // Update the desired opacity
        UpdateDesiredOpacity();

        // Update the color of the reload text
        UpdateColor();

        // Update the text of the reload text
        UpdateText();

        // Update the position of the reload text
        UpdatePosition();

        // Update the buttons
        UpdateButtons();
    }

    private void UpdateButtons()
    {
        // If there is no weapon manager, disable the buttons
        if (_weaponManager == null)
        {
            buttonsParent.SetActive(false);
            return;
        }

        // If the equipped gun is null, disable the buttons
        if (_weaponManager.EquippedGun == null)
        {
            buttonsParent.SetActive(false);
            return;
        }

        // If the equipped gun is not out of ammo, disable the buttons
        if (_weaponManager.EquippedGun.CurrentAmmo > 0)
        {
            buttonsParent.SetActive(false);
            return;
        }

        // If the equipped gun is out of ammo, enable the buttons
        buttonsParent.SetActive(true);

        // Set the buttons based on the current control scheme
        gamepadButtons.SetActive(InputManager.Instance.CurrentControlScheme == InputManager.ControlSchemeType.Gamepad);
        keyboardButtons.SetActive(InputManager.Instance.CurrentControlScheme == InputManager.ControlSchemeType.Keyboard);
    }

    private void UpdateInformation()
    {
        // Get the instance of the player object
        var player = Player.Instance;

        // Return if the player object is null
        if (player == null)
        {
            _weaponManager = null;
            return;
        }

        // Get the weapon manager component
        _weaponManager = player.WeaponManager;

        // Return if the weapon manager component is null
        if (_weaponManager == null)
            return;
    }

    private void UpdateDesiredOpacity()
    {
        // Set the desired opacity to 0 if the weapon manager is null
        if (_weaponManager == null)
            _desiredOpacity = 0;

        // If the equipped gun is null, set the desired opacity to 0
        else if (_weaponManager.EquippedGun == null)
            _desiredOpacity = 0;

        // Set the desired opacity to 1 if the weapon manager's weapon is out of ammo
        else if (_weaponManager.EquippedGun.CurrentAmmo == 0)
        {
            // If the player is not reloading, set the desired opacity to 1
            if (!_weaponManager.EquippedGun.IsReloading)
                _desiredOpacity = maxOpacity;

            // If the player is reloading, set the desired opacity to 0
            else
                _desiredOpacity = 0;
        }

        // Set the desired opacity to 0 if the weapon manager's weapon is not out of ammo
        else if (_weaponManager.EquippedGun.CurrentAmmo > 0)
        {
            // Get the ammo percentage
            var ammoPercentage = _weaponManager.EquippedGun.CurrentAmmo /
                                 (float)_weaponManager.EquippedGun.GunInformation.MagazineSize;

            // If the ammo percentage is less than or equal to the low ammo percentage,
            // set the desired opacity to 1
            if (ammoPercentage <= lowAmmoPercentage)
            {
                // Make the opacity blink
                var blinkAmount = Mathf.Sin(Time.time * lowAmmoBlinkFrequency) * 0.5f + 0.5f;

                // Set the desired opacity to the blink amount
                _desiredOpacity = blinkAmount;
            }

            else
                _desiredOpacity = 0;
        }

        // Lerp the alpha of the canvas group's alpha to the desired opacity
        canvasGroup.alpha =
            Mathf.Lerp(canvasGroup.alpha, _desiredOpacity, CustomFunctions.FrameAmount(opacityLerpAmount));

        // Set the alpha of the canvas group to the desired opacity if the difference between the two is less than the threshold
        if (Mathf.Abs(canvasGroup.alpha - _desiredOpacity) < LERP_THRESHOLD)
            canvasGroup.alpha = _desiredOpacity;
    }

    private void UpdateColor()
    {
        // Set the desired opacity to 0 if the weapon manager is null
        // Set the color of the reload text to white
        if (_weaponManager == null || _weaponManager.EquippedGun == null)
        {
            SetColor(Color.white);
            return;
        }

        // Get the ammo percentage
        var ammoPercentage = _weaponManager.EquippedGun.CurrentAmmo /
                             (float)_weaponManager.EquippedGun.GunInformation.MagazineSize;

        // Set the color of the reload text to the reload color
        if (_weaponManager.EquippedGun.CurrentAmmo == 0)
            SetColor(reloadColor);

        // If the ammo percentage is less than or equal to the low ammo percentage,
        // set the color of the reload text to the low ammo gradient
        else if (ammoPercentage <= lowAmmoPercentage)
        {
            // express the ammo percentage in terms of the low ammo gradient
            var gradientAmount = (ammoPercentage - lowAmmoGradientStop) / (lowAmmoPercentage - lowAmmoGradientStop);

            // Set the color of the reload text to the blink amount
            SetColor(lowAmmoGradient.Evaluate(1 - gradientAmount));
        }

        // Set the color of the reload text to white
        else
            SetColor(Color.white);
    }

    private void UpdateText()
    {
        // Return if there is no gun equipped
        if (_weaponManager == null || _weaponManager.EquippedGun == null)
            return;

        // Return if there is no tmp text component
        if (text == null)
            return;

        // Get the ammo percentage
        var ammoPercentage = _weaponManager.EquippedGun.CurrentAmmo /
                             (float)_weaponManager.EquippedGun.GunInformation.MagazineSize;

        // Set the text of the reload text to the low ammo text
        if (ammoPercentage <= lowAmmoPercentage)
            text.text = lowAmmoText;

        // Set the text of the reload text to the reload text
        if (_weaponManager.EquippedGun.CurrentAmmo == 0)
            text.text = reloadText;
    }

    private void SetColor(Color color)
    {
        // Lerp the color
        text.color = Color.Lerp(text.color, color, CustomFunctions.FrameAmount(.2f, false, true));
    }

    private void UpdatePosition()
    {
        // Return if the weapon manager is null
        if (_weaponManager == null)
            return;

        // Get the gun holder transform
        var gunHolderTransform = _weaponManager.GunHolder;

        // Return if the gun holder transform is null
        if (gunHolderTransform == null)
            return;

        // Bob the reload text
        var bobAmount = Mathf.Sin(Time.time * Mathf.PI * floatingBobFrequency) * floatingBobAmount;
        var bobOffset = new Vector3(0, bobAmount, 0);

        // Put the offset in terms of the gun holder's local space
        var relativeOffset = gunHolderTransform.TransformDirection(offset);

        // Set the position of the reload text to the gun holder's position
        var desiredPosition = gunHolderTransform.position + relativeOffset + bobOffset;

        // Lerp the position of the reload text to the desired position
        transform.position = Vector3.Lerp(transform.position, desiredPosition,
            CustomFunctions.FrameAmount(positionLerpAmount, false, true));
    }
}