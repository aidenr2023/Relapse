using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public class Spreadblast : MonoBehaviour, IPower
{
    [SerializeField] private GameObject spreadblastPrefab;

    public GameObject GameObject => gameObject;

    public PowerScriptableObject PowerScriptableObject { get; set; }

    public Sound NormalHitSfx => PowerScriptableObject.NormalHitSfx;
    public Sound CriticalHitSfx => PowerScriptableObject.CriticalHitSfx;

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
        var projectileCount = spreadblastPrefab.GetComponent<SpreadblastProjectile>().ProjectileCount;

        for (var i = 0; i < projectileCount; i++)
        {
            //Create object at current position
            var spreadBlast = Instantiate(spreadblastPrefab,
                powerManager.Player.PlayerController.CameraPivot.transform.position,
                powerManager.Player.PlayerController.CameraPivot.transform.rotation);

            var spreadblastProjectile = spreadBlast.GetComponent<SpreadblastProjectile>();

            // Create the position of the projectile
            var firePosition = powerManager.PowerFirePoint.position;

            // Create a vector that points forward from the camera pivot
            var aimTargetPoint = powerManager.PowerAimHitPoint;
            var fireForward = (aimTargetPoint - firePosition).normalized;

            spreadblastProjectile.Shoot(
                this, powerManager, pToken,
                firePosition, fireForward
            );
        }
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