using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallrunAnimationScript : MonoBehaviour
{
    private static readonly int IsWallrunRightAnimationID = Animator.StringToHash("isWallrunRight");
    private static readonly int IsWallrunLeftAnimationID = Animator.StringToHash("isWallrunLeft");


    // reference to Player Wallrunning
    public PlayerWallRunning playerWallRunning;

    // reference to the PlayerPowerManager
    [SerializeField] private PlayerPowerManager playerPowerManager;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private SyncMag7Fire syncMag7Fire;

    private void Update()
    {
        WallrunAnimation();
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