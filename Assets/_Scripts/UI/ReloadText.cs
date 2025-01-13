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

        // Update the position of the reload text
        UpdatePosition();
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
            _desiredOpacity = 0;

        // Lerp the alpha of the canvas group's alpha to the desired opacity
        const float defaultFrameTime = 1 / 60f;
        var frameAmount = Time.deltaTime / defaultFrameTime;
        canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, _desiredOpacity, opacityLerpAmount * frameAmount);

        // Set the alpha of the canvas group to the desired opacity if the difference between the two is less than the threshold
        if (Mathf.Abs(canvasGroup.alpha - _desiredOpacity) < LERP_THRESHOLD)
            canvasGroup.alpha = _desiredOpacity;
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
        var bobAmount = Mathf.Sin(Time.time * floatingBobFrequency) * floatingBobAmount;
        var bobOffset = new Vector3(0, bobAmount, 0);

        // Put the offset in terms of the gun holder's local space
        var relativeOffset = gunHolderTransform.TransformDirection(offset);
        
        // Debug.Log($"Offset: {offset} - Relative Offset: {relativeOffset}");

        // Set the position of the reload text to the gun holder's position
        var desiredPosition = gunHolderTransform.position + relativeOffset + bobOffset;

        const float defaultFrameTime = 1 / 60f;
        var frameAmount = Time.unscaledDeltaTime / defaultFrameTime;

        // Lerp the position of the reload text to the desired position
        transform.position = Vector3.Lerp(transform.position, desiredPosition, positionLerpAmount * frameAmount);
    }
}