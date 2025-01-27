using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public class Virus : MonoBehaviour, IPower
{
    public GameObject virusPrefab;

    public GameObject GameObject => gameObject;

    public PowerScriptableObject PowerScriptableObject { get; set; }

    public string PassiveEffectDebugText(PlayerPowerManager powerManager, PowerToken pToken)
    {
        return "";
    }

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
        
            //Create object at current position
            var virus = Instantiate(virusPrefab,
                powerManager.Player.PlayerController.CameraPivot.transform.position,
                powerManager.Player.PlayerController.CameraPivot.transform.rotation);

            var virusProjectile = virus.GetComponent<VirusProjectile>();
        
            // Create the position of the projectile
            var firePosition = powerManager.PowerFirePoint.position;

            // Create a vector that points forward from the camera pivot
            var aimTargetPoint = powerManager.PowerAimHitPoint;
            var fireForward = (aimTargetPoint - firePosition).normalized;

            virusProjectile.Shoot(
                this, powerManager, pToken,
                // powerManager.Player.WeaponManager.FireTransform.position,
                // powerManager.Player.WeaponManager.FireTransform.forward
                firePosition, fireForward
            );
        
    }

    public void StartActiveEffect(PlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    public void UpdateActiveEffect(PlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    public void EndActiveEffect(PlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    public void StartPassiveEffect(PlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    public void UpdatePassiveEffect(PlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    public void EndPassiveEffect(PlayerPowerManager powerManager, PowerToken pToken)
    {
    }
}