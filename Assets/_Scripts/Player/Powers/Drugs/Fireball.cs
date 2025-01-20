using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : MonoBehaviour, IPower
{
    // a field for a projectile prefab
    [SerializeField] private GameObject projectilePrefab;

    public GameObject GameObject => gameObject;
    public PowerScriptableObject PowerScriptableObject { get; set; }

    public string PassiveEffectDebugText(PlayerPowerManager powerManager, PowerToken pToken) => string.Empty;

    #region IPower Methods

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
        // Create the position of the projectile
        var firePosition = powerManager.PowerFirePoint.position;
     
        // Create a vector that points forward from the camera pivot
        var aimTargetPoint = powerManager.PowerAimHitPoint;
        var fireForward = (aimTargetPoint - firePosition).normalized;
        
        // Instantiate the projectile prefab
        var projectile = Instantiate(projectilePrefab, firePosition, Quaternion.identity);

        // Get the IPowerProjectile component from the projectile
        var powerProjectile = projectile.GetComponent<IPowerProjectile>();

        // Shoot the projectile
        powerProjectile.Shoot(this, powerManager, pToken, firePosition, fireForward);

        // // Create the projectile
        // var projectileScript = CreateProjectile(firePosition, fireForward);
        //
        // // Set up the projectile
        // SetUpProjectile(powerManager, projectileScript);
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