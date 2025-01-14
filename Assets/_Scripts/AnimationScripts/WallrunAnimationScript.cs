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
            playerAnimator.SetBool("isWallrunLeft", true);
        }
        else if (playerWallRunning.IsWallRunningLeft)
        {
            //set the wallrun animation to true
            playerAnimator.SetBool("isWallrunRight", true);
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
    
  
}
