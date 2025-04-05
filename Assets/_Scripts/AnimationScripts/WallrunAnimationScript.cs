using System;
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
    private static readonly int PowerChargeAnimationTypeAnimationID = Animator.StringToHash("PowerTypeChargingBlend");

    // reference to Player Wallrunning
    public PlayerWallRunning playerWallRunning;

    // reference to the PlayerPowerManager
    public PlayerPowerManager playerPowerManager;
    public Animator playerAnimator;
    public SyncMag7Fire syncMag7Fire;

    private void Update()
    {
        WallrunAnimation();
        ChargingAnimation();
        PowerAnimation();
    }

    public void WallrunAnimation()
    {
        // if the player is wallrunning
        // set the wallrun animation to true
        if (playerWallRunning.IsWallRunningRight)
            playerAnimator.SetBool(IsWallrunRightAnimationID, true);

        // set the wallrun animation to true
        else if (playerWallRunning.IsWallRunningLeft)
            playerAnimator.SetBool(IsWallrunLeftAnimationID, true);

        // set the wallrun animation to false
        else
        {
            playerAnimator.SetBool(IsWallrunLeftAnimationID, false);
            playerAnimator.SetBool(IsWallrunRightAnimationID, false);
        }
    }

    private void PowerAnimation()
    {
        //set the trigger for the power animation
        if (playerPowerManager.WasPowerJustUsed)
        {
            playerAnimator.SetInteger(AnimationTypeAnimationID, (int)playerPowerManager.CurrentPower.AnimationType);
            playerAnimator.SetTrigger(WasPowerJustUsedAnimationID);
        }

        playerAnimator.SetBool(ChargingAnimationID, playerPowerManager.IsChargingPower);
    }

    private void ChargingAnimation()
    {
        if (playerPowerManager.CurrentPower != null)
            playerAnimator.SetFloat(PowerChargeAnimationTypeAnimationID,
                (int)playerPowerManager.CurrentPower.ChargeAnimationType);
    }

    //based on the flag in sync mag7 fire, play the trigger
    public void PlayTriggerOnShoot(GameObject obj)
    {
        // on shoot invoke this method

        // Find the Animator in the children of the GameObject
        var mag7animator = obj.GetComponentInChildren<Animator>();

        // Set the trigger
        //Mag7animator.SetTrigger("Shooting");
        playerAnimator.SetTrigger("Cocking");
    }
}