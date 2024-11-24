using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour, IPower
{
    public GameObject grenadePrefab;

    public GameObject GameObject => gameObject;

    public PowerScriptableObject PowerScriptableObject { get; set; }

    void Start()
    {
    }

    void Update()
    {
    }

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
        var grenade = Instantiate(grenadePrefab,
            powerManager.Player.PlayerController.CameraPivot.transform.position,
            powerManager.Player.PlayerController.CameraPivot.transform.rotation);

        var grenadeProjectile = grenade.GetComponent<GrenadeProjectile>();

        grenadeProjectile.Shoot(
            this, powerManager, pToken,
            powerManager.Player.WeaponManager.FireTransform.position,
            powerManager.Player.WeaponManager.FireTransform.forward
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