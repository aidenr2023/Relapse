using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDrug : MonoBehaviour, IPower
{
    private bool _isCharging;
    private float _currentChargeDuration;

    public PowerScriptableObject PowerScriptableObject { get; set; }

    /// <summary>
    /// TODO: Change this to the actual tolerance meter impact (level dependent)
    /// </summary>
    public float ToleranceMeterImpact => 1;

    public float ChargePercentage
    {
        get
        {
            // If there is no charge duration, return 0 or 1
            if (PowerScriptableObject.ChargeDuration == 0)
                return _currentChargeDuration > 0 ? 1 : 0;

            // Otherwise, return the charge percentage
            return _currentChargeDuration / PowerScriptableObject.ChargeDuration;
        }
    }


    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }


    #region IPower

    public void StartCharge(bool startedChargingThisFrame)
    {
        Debug.Log(
            startedChargingThisFrame
                ? $"Started Charging This Frame!"
                : $"Started Charging This Some Other Frame!"
        );

        // Set the charging flag to true
        _isCharging = true;
    }

    public void Charge()
    {
        // Charge the power
        this.ChargePowerDuration(ref _currentChargeDuration);

        Debug.Log($"Charging This: {ChargePercentage}!");
    }

    public void Release(bool isCharged)
    {
        Debug.Log(
            isCharged
                ? $"Released This Fully Charged!"
                : $"Released This Not Fully Charged!"
        );

        // Set the charging flag to false
        _isCharging = false;

        // Reset the charge duration if the power is not charging
        _currentChargeDuration = 0;
    }

    public void Use()
    {
        Debug.Log($"Using This!");

        // Reset the charge duration if the power is not charging
        _currentChargeDuration = 0;
    }

    #endregion
}