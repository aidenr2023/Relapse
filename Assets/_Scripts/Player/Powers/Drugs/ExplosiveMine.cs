using Unity.VisualScripting;
using UnityEngine;

public class ExplosiveMine : MonoBehaviour, IPower
{
    #region Serialized Fields

    [SerializeField] private ExplosiveMineProjectile explosiveMineProjectilePrefab;

    #endregion

    #region Getters

    public GameObject GameObject => gameObject;
    public PowerScriptableObject PowerScriptableObject { get; set; }

    #endregion

    #region IPower Methods

    public string PassiveEffectDebugText(PlayerPowerManager powerManager, PowerToken pToken) => string.Empty;

    public void StartCharge(PlayerPowerManager powerManager, PowerToken pToken, bool startedChargingThisFrame)
    {
    }

    public void Charge(PlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    public void Release(PlayerPowerManager powerManager, PowerToken pToken, bool isCharged)
    {
    }

    public void Use(PlayerPowerManager powerManager, PowerToken pToken)
    {
        // Create a vector that points forward from the camera pivot
        var fireForward = powerManager.Player.PlayerController.CameraPivot.transform.forward;

        // Create the position of the projectile
        var firePosition = powerManager.Player.PlayerController.CameraPivot.transform.position + fireForward * 1;

        // Instantiate the projectile prefab
        var projectile = Instantiate(explosiveMineProjectilePrefab, firePosition, Quaternion.identity);

        // Shoot the projectile
        projectile.Shoot(this, powerManager, pToken, firePosition, fireForward);
    }

    #region Active Effects

    public void StartActiveEffect(PlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    public void UpdateActiveEffect(PlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    public void EndActiveEffect(PlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    #endregion


    #region Passive Effects

    public void StartPassiveEffect(PlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    public void UpdatePassiveEffect(PlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    public void EndPassiveEffect(PlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    #endregion

    #endregion
}