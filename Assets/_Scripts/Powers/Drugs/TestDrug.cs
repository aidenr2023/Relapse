using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDrug : MonoBehaviour, IPower
{
    public GameObject GameObject => gameObject;

    public PowerScriptableObject PowerScriptableObject { get; set; }

    #region IPower

    public void StartCharge(TestPlayerPowerManager powerManager, PowerToken pToken, bool startedChargingThisFrame)
    {
        Debug.Log(
            startedChargingThisFrame
                ? $"Started Charging This Frame!"
                : $"Started Charging This Some Other Frame!"
        );
    }

    public void Charge(TestPlayerPowerManager powerManager, PowerToken pToken)
    {
        Debug.Log($"Charging This!");
    }

    public void Release(TestPlayerPowerManager powerManager, PowerToken pToken, bool isCharged)
    {
        Debug.Log(isCharged
            ? $"Released This Fully Charged!"
            : $"Released This Not Fully Charged!"
        );
    }

    public void Use(TestPlayerPowerManager powerManager, PowerToken pToken)
    {
        Debug.Log($"Using This!");
    }

    public void StartActiveEffect(TestPlayerPowerManager powerManager, PowerToken pToken)
    {
        Debug.Log($"Starting Active Effect!");
    }

    public void UpdateActiveEffect(TestPlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    public void EndActiveEffect(TestPlayerPowerManager powerManager, PowerToken pToken)
    {
        Debug.Log($"Ending Active Effect!");
    }

    public void StartPassiveEffect(TestPlayerPowerManager powerManager, PowerToken pToken)
    {
        Debug.Log($"Starting Passive Effect!");
    }

    public void UpdatePassiveEffect(TestPlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    public void EndPassiveEffect(TestPlayerPowerManager powerManager, PowerToken pToken)
    {
        Debug.Log($"Ending Passive Effect!");
    }

    #endregion
}