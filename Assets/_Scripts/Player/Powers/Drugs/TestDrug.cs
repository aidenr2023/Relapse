using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDrug : MonoBehaviour, IPower
{
    public GameObject GameObject => gameObject;

    public PowerScriptableObject PowerScriptableObject { get; set; }

    public string PassiveEffectDebugText(PlayerPowerManager powerManager, PowerToken pToken) => string.Empty;

    #region IPower

    public void StartCharge(PlayerPowerManager powerManager, PowerToken pToken, bool startedChargingThisFrame)
    {
        // Debug.Log(
        //     startedChargingThisFrame
        //         ? $"Started Charging This Frame!"
        //         : $"Started Charging This Some Other Frame!"
        // );
    }

    public void Charge(PlayerPowerManager powerManager, PowerToken pToken)
    {
        // Debug.Log($"Charging This!");
    }

    public void Release(PlayerPowerManager powerManager, PowerToken pToken, bool isCharged)
    {
        // Debug.Log(isCharged
        //     ? $"Released This Fully Charged!"
        //     : $"Released This Not Fully Charged!"
        // );
    }

    public void Use(PlayerPowerManager powerManager, PowerToken pToken)
    {
        // Debug.Log($"Using This!");
        Debug.Log($"This DRUG does nothing!");
    }

    public void StartActiveEffect(PlayerPowerManager powerManager, PowerToken pToken)
    {
        // Debug.Log($"Starting Active Effect!");
    }

    public void UpdateActiveEffect(PlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    public void EndActiveEffect(PlayerPowerManager powerManager, PowerToken pToken)
    {
        // Debug.Log($"Ending Active Effect!");
    }

    public void StartPassiveEffect(PlayerPowerManager powerManager, PowerToken pToken)
    {
        // Debug.Log($"Starting Passive Effect!");
    }

    public void UpdatePassiveEffect(PlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    public void EndPassiveEffect(PlayerPowerManager powerManager, PowerToken pToken)
    {
        // Debug.Log($"Ending Passive Effect!");
    }

    #endregion
}