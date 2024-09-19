using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDrug : MonoBehaviour, IPower
{
    public PowerScriptableObject PowerScriptableObject { get; set; }

    /// <summary>
    /// TODO: Change this to the actual tolerance meter impact (level dependent)
    /// </summary>
    public float ToleranceMeterImpact => 1;

    #region IPower

    public void StartCharge(TestPlayerPowerManager powerManager, PowerToken pToken, bool startedChargingThisFrame)
    {
        Debug.Log(
            startedChargingThisFrame
                ? $"Started Charging This Frame!"
                : $"Started Charging This Some Other Frame!"
        );

        // Set the charging flag to true
        pToken.SetChargingFlag(true);
    }

    public void Charge(TestPlayerPowerManager powerManager, PowerToken pToken)
    {
        // Charge the power
        pToken.ChargePowerDuration();
    }

    public void Release(TestPlayerPowerManager powerManager, PowerToken pToken, bool isCharged)
    {
        Debug.Log(
            isCharged
                ? $"Released This Fully Charged!"
                : $"Released This Not Fully Charged!"
        );

        // Set the charging flag to false
        pToken.SetChargingFlag(false);

        // Reset the charge duration if the power is not charging
        pToken.ResetChargeDuration();
    }

    public void Use(TestPlayerPowerManager powerManager, PowerToken pToken)
    {
        Debug.Log($"Using This!");

        // // Reset the charge duration if the power is not charging
        // _currentChargeDuration = 0;
    }

    #endregion
}