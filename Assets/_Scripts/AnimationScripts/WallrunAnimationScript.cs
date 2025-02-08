using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallrunAnimationScript : MonoBehaviour
{
    //reference to Player Wallrunning
    public PlayerWallRunning playerWallRunning;
    
    //reference to the PlayerPowerManager
    public PlayerPowerManager playerPowerManager;
    public Animator playerAnimator;
    public SyncMag7Fire syncMag7Fire;
    
    private void Update()
    {
        WallrunAnimation();
        PowerAnimation();
    }
    public void WallrunAnimation()
    {
        //if the player is wallrunning
        if (playerWallRunning.IsWallRunningRight)
        {
            //set the wallrun animation to true
            playerAnimator.SetBool("isWallrunRight", true);
        }
        else if (playerWallRunning.IsWallRunningLeft)
        {
            //set the wallrun animation to true
            playerAnimator.SetBool("isWallrunLeft", true);
        }
        else
        {
            //set the wallrun animation to false
            playerAnimator.SetBool("isWallrunLeft", false);
            playerAnimator.SetBool("isWallrunRight", false);
        }
    }
    
    public void PowerAnimation()
    {
        //set the trigger for the power animation
        if(playerPowerManager.WasPowerJustUsed)
        {
            playerAnimator.SetTrigger("wasPowerJustUsed");
        }
        
        playerAnimator.SetBool("Charging",    playerPowerManager.IsChargingPower);
        
    }
    //based on the flag in sync mag7 fire, play the trigger
    public void PlayTriggerOnShoot(GameObject obj)
    {
        //on shoot invoke this method
        
        // Find the Animator in the children of the GameObject
        Animator Mag7animator = obj.GetComponentInChildren<Animator>();
        // Set the trigger
        //Mag7animator.SetTrigger("Shooting");
        playerAnimator.SetTrigger("Cocking");
       
    }
    
    
  
}
