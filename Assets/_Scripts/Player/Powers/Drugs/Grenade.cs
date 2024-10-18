using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour, IPower
{
    public GameObject grenadePrefab;
    
    //private AudioSource _fusrodah;


    public GameObject GameObject => gameObject;

    public PowerScriptableObject PowerScriptableObject { get; set; }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
    
    }
    public void FusRoDahShout()
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
        
        Debug.Log("Key activated, preparing to fire");

        //Begin FusRoDah shout
        Debug.Log("yell");

        //Create object at current position
        GameObject grenade = Instantiate(grenadePrefab, powerManager.Player.PlayerController.CameraPivot.transform.position, powerManager.Player.PlayerController.CameraPivot.transform.rotation);

        //Play sound
        /*
        _fusrodah = GetComponent<AudioSource>();
        if (_fusrodah != null)
        {
            _fusrodah.Play();
        }
        */

        //Destroy after time
        //Destroy(grenade, 10f);

        grenade.GetComponent<GrenadeProjectile>().Shoot(this,powerManager,pToken,default,default);
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