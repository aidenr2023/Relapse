using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AmmoText : MonoBehaviour
{
    private const float LERP_THRESHOLD = 0.0001f;

    #region Serialized Fields

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private ReloadText reloadText;

    [SerializeField] private CanvasGroup textCanvasGroup;
    [SerializeField] private CanvasGroup iconCanvasGroup;
    [SerializeField] private Transform iconTransform;

    [SerializeField, Range(0, 1)] private float maxOpacity = 1;
    [SerializeField, Range(0, 1)] private float opacityLerpAmount = 0.15f;
    [SerializeField, Range(0, 1)] private float positionLerpAmount = 0.15f;

    [SerializeField] private Vector3 offset;
    [SerializeField] private TMP_Text text;

    [SerializeField, Min(0)] private float rotationSpeed = 2;

    [SerializeField] private Image ammoCountFillImage;
    [SerializeField, Range(0, 1)] private float fillAmountLerpAmount = 0.35f;
    [SerializeField] private CanvasGroup ammoCountFillImageGroup;

    #endregion

    #region Private Fields

    private WeaponManager _weaponManager;
    private float _desiredOpacity;

    #endregion

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

        // Update the visibility of the reload text
        UpdateVisibility();

        // Update the ammo count fill image
        UpdateAmmoCountFillImage();
    }

    private void UpdateAmmoCountFillImage()
    {
        // Return if the ammo count fill image is null
        if (ammoCountFillImage == null)
            return;

        // Return if the weapon manager is null
        if (_weaponManager == null)
            return;

        // Return if the equipped gun is null
        if (_weaponManager.EquippedGun == null)
            return;

        var desiredFillPercentage = _weaponManager.EquippedGun.CurrentAmmo /
                                    (float)_weaponManager.EquippedGun.GunInformation.MagazineSize;

        // Set the fill amount of the ammo count fill image to the percentage of the equipped gun's ammo
        ammoCountFillImage.fillAmount = Mathf.Lerp(
            ammoCountFillImage.fillAmount, desiredFillPercentage,
            CustomFunctions.FrameAmount(fillAmountLerpAmount, false, true)
        );

        if (Mathf.Abs(ammoCountFillImage.fillAmount - desiredFillPercentage) < LERP_THRESHOLD)
            ammoCountFillImage.fillAmount = desiredFillPercentage;

        ammoCountFillImage.color = reloadText.Color;

        var desiredOpacity = _weaponManager.EquippedGun.IsReloading ? 0 : 1;

        ammoCountFillImageGroup.alpha = Mathf.Lerp(
            ammoCountFillImageGroup.alpha, desiredOpacity,
            CustomFunctions.FrameAmount(opacityLerpAmount * 2, false, true)
        );
        
        if (Mathf.Abs(ammoCountFillImageGroup.alpha - desiredOpacity) < LERP_THRESHOLD)
            ammoCountFillImageGroup.alpha = desiredOpacity;
    }

    private void UpdateVisibility()
    {
        // If the player is currently reloading, set the visibility of the text canvas group to 0
        if (_weaponManager != null && _weaponManager.EquippedGun != null && _weaponManager.EquippedGun.IsReloading)
        {
            textCanvasGroup.alpha = 0;
            iconCanvasGroup.alpha = 1;
        }
        else
        {
            textCanvasGroup.alpha = 1;
            iconCanvasGroup.alpha = 0;
        }

        // Rotate the icon canvas group
        iconTransform.Rotate(Vector3.forward, 360 * Time.deltaTime * rotationSpeed);
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
        _desiredOpacity = 1;

        // Set the desired opacity to 0 if the weapon manager is null
        if (_weaponManager == null)
            _desiredOpacity = 0;

        // If the equipped gun is null, set the desired opacity to 0
        else if (_weaponManager.EquippedGun == null)
            _desiredOpacity = 0;
        
        // If the equipped gun does not use ammo, set the desired opacity to 0
        else if (!_weaponManager.EquippedGun.GunInformation.UseAmmo)
            _desiredOpacity = 0;

        // Lerp the alpha of the canvas group's alpha to the desired opacity
        const float defaultFrameTime = 1 / 60f;
        var frameAmount = Time.deltaTime / defaultFrameTime;
        canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, _desiredOpacity, opacityLerpAmount * frameAmount);

        // Set the alpha of the canvas group to the desired opacity if the difference between the two is less than the threshold
        if (Mathf.Abs(canvasGroup.alpha - _desiredOpacity) < LERP_THRESHOLD)
            canvasGroup.alpha = _desiredOpacity;
    }

    private void UpdateColor()
    {
        // Set the color of the text to red if the equipped gun is out of ammo
        text.color = reloadText.Color;
    }

    private void UpdateText()
    {
        // Return if the weapon manager is null
        if (_weaponManager == null)
            return;

        // Return if the equipped gun is null
        if (_weaponManager.EquippedGun == null)
            return;

        // Set the text of the ammo text to the ammo count of the equipped gun
        text.text = $"{_weaponManager.EquippedGun.CurrentAmmo}";

        // Set the text of the ammo text to "Reloading" if the equipped gun is reloading
        if (_weaponManager.EquippedGun.IsReloading)
        {
            var padAmount = (int)(_weaponManager.EquippedGun.ReloadingPercentage / .33f + 1);
            text.text = "".PadRight(padAmount, '.');
        }
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

        // Put the offset in terms of the gun holder's local space
        var relativeOffset = gunHolderTransform.TransformDirection(offset);

        // Set the position of the reload text to the gun holder's position
        var desiredPosition = gunHolderTransform.position + relativeOffset;

        const float defaultFrameTime = 1 / 60f;
        var frameAmount = Time.unscaledDeltaTime / defaultFrameTime;

        // Lerp the position of the reload text to the desired position
        transform.position = Vector3.Lerp(transform.position, desiredPosition, positionLerpAmount * frameAmount);
    }
}