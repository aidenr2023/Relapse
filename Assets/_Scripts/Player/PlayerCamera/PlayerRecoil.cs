using System;
using UnityEngine;

public class PlayerRecoil : ComponentScript<PlayerVirtualCameraController>
{
    #region Serialized Fields

    [SerializeField] private Transform cameraPivotParent;

    [SerializeField, Min(0)] private float recoveryThreshold = 0.01f;

    [SerializeField, Min(0)] private float maxXRecoil = 90;
    [SerializeField, Min(0)] private float maxYRecoil = 90;

    #endregion

    #region Private Fields

    private TokenManager<Vector3>.ManagedToken _recoilToken;

    private Vector3 _desiredRecoil;

    private bool _isRecovering = true;

    private GunInformation _gunInformation;

    #endregion

    #region Getters

    private float HorizontalRecoil => _gunInformation?.HorizontalRecoil ?? 0;
    private float VerticalRecoil => _gunInformation?.VerticalRecoil ?? 0;

    private float HorizontalRecoilBias => _gunInformation?.HorizontalRecoilBias ?? 0;

    private float RecoilLerpAmount => _gunInformation?.RecoilLerpAmount ?? 1;
    private float RecoveryLerpAmount => _gunInformation?.RecoveryLerpAmount ?? 1;

    private float MinHorizontalRecoilPercent => _gunInformation?.MinHorizontalRecoilPercent ?? 0;
    private float MinVerticalRecoilPercent => _gunInformation?.MinVerticalRecoilPercent ?? 0;

    #endregion

    private void Start()
    {
        // Initialize the recoil token
        _recoilToken = new TokenManager<Vector3>.ManagedToken(Vector3.zero, -1, true);

        ParentComponent.ParentComponent.WeaponManager.OnGunRemoved += (_, _) => _isRecovering = true;
    }

    private void Update()
    {
        if (ParentComponent.ParentComponent.WeaponManager.EquippedGun == null)
            _gunInformation = null;

        // Update the recoil token
        UpdateRecoilToken();

        // Update the camera pivot parent
        cameraPivotParent.localEulerAngles = _recoilToken.Value;
    }

    private void UpdateRecoilToken()
    {
        // If the gun information is null, force the recovery flag to true
        if (_gunInformation == null)
            _isRecovering = true;

        const float defaultFrameTime = 1 / 60f;
        var frameAmount = Time.deltaTime / defaultFrameTime;

        // Lerp the recoil back to zero
        if (_isRecovering)
            _recoilToken.Value = Vector3.Lerp(_recoilToken.Value, Vector3.zero, RecoveryLerpAmount * frameAmount);
        else
        {
            // Lerp the recoil to the desired recoil
            _recoilToken.Value = Vector3.Lerp(_recoilToken.Value, _desiredRecoil, RecoilLerpAmount * frameAmount);

            // Check if the recoil has been recovered
            // Set the recovery flag to true
            if (Vector3.Distance(_recoilToken.Value, _desiredRecoil) < recoveryThreshold)
                _isRecovering = true;
        }
    }

    private void AddRecoil(Vector3 recoil)
    {
        // Calculate the random horizontal recoil
        var minHorizontalRecoilLower =
            HorizontalRecoilBias * (HorizontalRecoil / 2) - HorizontalRecoil / 2;
        var minHorizontalRecoilUpper =
            HorizontalRecoilBias * (HorizontalRecoil / 2) - (HorizontalRecoil * MinHorizontalRecoilPercent) / 2;
        var maxHorizontalRecoilUpper =
            HorizontalRecoilBias * (HorizontalRecoil / 2) + HorizontalRecoil / 2;
        var maxHorizontalRecoilLower =
            HorizontalRecoilBias * (HorizontalRecoil / 2) + (HorizontalRecoil * MinHorizontalRecoilPercent) / 2;

        // Randomize the horizontal recoil using the lower and upper bounds
        var randomHorizontalRecoil = UnityEngine.Random.Range(0, 2) == 0
            ? UnityEngine.Random.Range(minHorizontalRecoilLower, minHorizontalRecoilUpper)
            : UnityEngine.Random.Range(maxHorizontalRecoilLower, maxHorizontalRecoilUpper);

        var randomVerticalRecoil =
            UnityEngine.Random.Range(VerticalRecoil * MinVerticalRecoilPercent, VerticalRecoil);

        // Add the recoil to the desired recoil
        _desiredRecoil = _recoilToken.Value + new Vector3(-randomVerticalRecoil, randomHorizontalRecoil, recoil.z);

        // Clamp the desired recoil based on the max recoil values
        _desiredRecoil = new Vector3(
            Mathf.Clamp(_desiredRecoil.x, -maxXRecoil, maxXRecoil),
            Mathf.Clamp(_desiredRecoil.y, -maxYRecoil, maxYRecoil),
            _desiredRecoil.z
        );

        // Set the recovery flag to false
        _isRecovering = false;
    }

    public void AddRecoil(GunInformation gunInformation)
    {
        // Return if the gun information is null
        if (gunInformation == null)
            return;

        // Set the gun information
        _gunInformation = gunInformation;

        AddRecoil(new Vector3(VerticalRecoil, HorizontalRecoil, 0));
    }
}