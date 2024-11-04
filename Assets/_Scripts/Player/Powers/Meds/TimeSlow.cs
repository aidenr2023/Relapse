using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeSlow : MonoBehaviour, IPower
{
    //Time scale variables
    [SerializeField][Range(0.01f, 1)] private float timeScaleAdjust = 1;
    private int defaultTime = 1;
    private float fixedDeltaTime;

    public GameObject GameObject => gameObject;

    public PowerScriptableObject PowerScriptableObject { get; set; }

    void Start()
    {

    }
    void Awake()
    {
        this.fixedDeltaTime = Time.fixedDeltaTime;
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
        //Begin timeslow
        Debug.Log("Time Slow");

        //Set the time scale to the adjusted time scale from the inspector
        Time.timeScale = timeScaleAdjust;

        //Do this to make sure the physics don't break
        Time.fixedDeltaTime = this.fixedDeltaTime * Time.timeScale;



        //Fancy animate crap


    }

    public void UpdatePassiveEffect(PlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    public void EndPassiveEffect(PlayerPowerManager powerManager, PowerToken pToken)
    {
        Debug.Log("Time Slow Done");

        //Set timescale back to default (should be 1)
        Time.timeScale = defaultTime;

        //Do this to make sure the physics don't break
        Time.fixedDeltaTime = fixedDeltaTime * Time.timeScale;

    }
}