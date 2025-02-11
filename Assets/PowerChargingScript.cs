using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerChargingScript : MonoBehaviour
{
    //reference to the player power manager
    private PlayerPowerManager _playerPowerManager;
    private Animator _playerAnimator;
    
    private void Start()
    {
        //get the player power manager
        _playerPowerManager = GetComponent<PlayerPowerManager>();
        
        //get the player animator
        _playerAnimator = GetComponent<Animator>();
    }
    
    private void Update()
    {
        //call the power animation function
        PowerAnimation();
    }
    
    
    public void PowerAnimation()
    {
        //set the trigger for the power animation
        if(_playerPowerManager.IsChargingPower)
        {
            _playerAnimator.SetBool("Charging",    _playerPowerManager.IsChargingPower);
            
        }
        
    }
    public void PauseAnimation()
    {
        _playerAnimator.speed = 0f;
    }

    // Call this when the player releases the input.
    public void ResumeAnimation()
    {
        //on power release, set the speed of the player animator to 1
        
        _playerAnimator.speed = 1f;
    }
}
