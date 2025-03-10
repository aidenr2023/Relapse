using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeSlow : MonoBehaviour, IPower
{
    private const string TIME_TOKEN_DATA_KEY = "TimeSlow";

    #region Serialized Fields

    [SerializeField] [Range(0.01f, 1)] private float timeScaleAdjust = 1;

    #endregion

    #region Getters

    public GameObject GameObject => gameObject;

    public PowerScriptableObject PowerScriptableObject { get; set; }
    
    public Sound NormalHitSfx => PowerScriptableObject.NormalHitSfx;
    public Sound CriticalHitSfx => PowerScriptableObject.CriticalHitSfx;


    #endregion

    void Awake()
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

    private void CreateEffectToken(PlayerPowerManager powerManager, PowerToken pToken)
    {
        // Begin time slow
        // Set the timeScale to the adjusted timeScale from the inspector
        var timeToken = TimeScaleManager.Instance.TimeScaleTokenManager.AddToken(timeScaleAdjust, -1, true);

        // Add the time token to the power token's data
        pToken.AddData(TIME_TOKEN_DATA_KEY, timeToken);

        // // Do this to make sure the physics don't break
        // Time.fixedDeltaTime = this._fixedDeltaTime * Time.timeScale;
    }

    public void StartPassiveEffect(PlayerPowerManager powerManager, PowerToken pToken)
    {
        CreateEffectToken(powerManager, pToken);
    }

    public void UpdatePassiveEffect(PlayerPowerManager powerManager, PowerToken pToken)
    {
        var hasData = pToken.TryGetData<TokenManager<float>.ManagedToken>(TIME_TOKEN_DATA_KEY, out var timeToken);

        // Ensure the player has the time token
        if (!hasData && !TimeScaleManager.Instance.TimeScaleTokenManager.HasToken(timeToken))
            CreateEffectToken(powerManager, pToken);
    }

    public void EndPassiveEffect(PlayerPowerManager powerManager, PowerToken pToken)
    {
        // Retrieve the time token from the power token's data
        _ = pToken.RemoveData<TokenManager<float>.ManagedToken>(TIME_TOKEN_DATA_KEY, out var timeToken);

        // Remove the time token from the timeScale manager
        TimeScaleManager.Instance.TimeScaleTokenManager.RemoveToken(timeToken);

        // // Do this to make sure the physics don't break
        // Time.fixedDeltaTime = _fixedDeltaTime * Time.timeScale;
    }
}