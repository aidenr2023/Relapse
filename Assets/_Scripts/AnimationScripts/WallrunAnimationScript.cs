using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallrunAnimationScript : MonoBehaviour
{
    private static readonly int IsWallrunRightAnimationID = Animator.StringToHash("isWallrunRight");
    private static readonly int IsWallrunLeftAnimationID = Animator.StringToHash("isWallrunLeft");
    private static readonly int WasPowerJustUsedAnimationID = Animator.StringToHash("wasPowerJustUsed");
    private static readonly int ChargingAnimationID = Animator.StringToHash("Charging");
    private static readonly int AnimationTypeAnimationID = Animator.StringToHash("PowerAnimationType");

    // reference to Player Wallrunning
    public PlayerWallRunning playerWallRunning;

    // reference to the PlayerPowerManager
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
            playerAnimator.SetBool(IsWallrunRightAnimationID, true);
        }
        else if (playerWallRunning.IsWallRunningLeft)
        {
            //set the wallrun animation to true
            playerAnimator.SetBool(IsWallrunLeftAnimationID, true);
        }
        else
        {
            //set the wallrun animation to false
            playerAnimator.SetBool(IsWallrunLeftAnimationID, false);
            playerAnimator.SetBool(IsWallrunRightAnimationID, false);
        }
    }

    public void PowerAnimation()
    {
        //set the trigger for the power animation
        if (playerPowerManager.WasPowerJustUsed)
        {
            playerAnimator.SetInteger(AnimationTypeAnimationID, (int) playerPowerManager.CurrentPower.AnimationType);
            playerAnimator.SetTrigger(WasPowerJustUsedAnimationID);
        }

        playerAnimator.SetBool(ChargingAnimationID, playerPowerManager.IsChargingPower);
    }

    //based on the flag in sync mag7 fire, play the trigger
    public void PlayTriggerOnShoot(GameObject obj)
    {
        // on shoot invoke this method

        // Find the Animator in the children of the GameObject
        var Mag7animator = obj.GetComponentInChildren<Animator>();
        
        // Set the trigger
        //Mag7animator.SetTrigger("Shooting");
        playerAnimator.SetTrigger("Cocking");
    }
}