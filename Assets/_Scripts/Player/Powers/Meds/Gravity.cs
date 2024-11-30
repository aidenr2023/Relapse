using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gravity : MonoBehaviour, IPower
{
    //Time scale variables
    [SerializeField][Range(0.01f, 1)] private float timeScaleAdjust = 1;
    

    public GameObject GameObject => gameObject;

    public PowerScriptableObject PowerScriptableObject { get; set; }

    void Start()
    {
        //Get the player component
    }
    void Awake()
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
        Debug.Log("Gravity Reduced");

        //Set player rigid body gravity to off


    }
    /*IEnumerator Example()
    {
        float timeA = Time.time;
        Debug.Log("Initial: " + (timeA));

        yield return new WaitForSecondsRealtime(secondsBetweenFrames);
        float timeB = Time.time;
        Debug.Log("Final: " + (timeB));
        
        float timedif = timeB - timeA;
        Debug.Log("Time Difference: " + timedif);

    }
    */

    public void UpdatePassiveEffect(PlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    public void EndPassiveEffect(PlayerPowerManager powerManager, PowerToken pToken)
    {
        Debug.Log("Gravity Reduction Done");

        //Set player gravity rigid body to on

    }
}