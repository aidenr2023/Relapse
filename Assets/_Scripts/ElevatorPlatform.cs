using System;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorPlatform : MonoBehaviour
{
    private static int _elevatorPlatformCount;

    [SerializeField] private bool disableJumping;
    
    private readonly HashSet<Player> _playersOnThisPlatform = new();

    private void OnTriggerStay(Collider other)
    {
        // Return if the collider is not a root object
        if (other.transform.parent != null)
            return;

        // If the object does not have a player component, return
        if (!other.TryGetComponent(out Player player))
            return;

        // If the player is not grounded, return
        if (!player.PlayerController.IsGrounded)
            return;
        
        // If the player is already on the platform, return
        // Add the player to the platform
        if (!_playersOnThisPlatform.Add(player))
            return;
        
        // Increment the elevator platform count
        _elevatorPlatformCount++;

        // Make this game object the parent of the player
        player.transform.SetParent(transform);
        
        // // If this elevator disables jumping, disable the player's jumping
        // if (disableJumping)
        //     (player.PlayerController as PlayerMovementV2)?.BasicPlayerMovement.AddJumpDisabler(this);
    }

    private void OnTriggerExit(Collider other)
    {
        // If the object does not have a player component, return
        if (!other.TryGetComponent(out Player player))
            return;

        // If the player is not on the platform, return
        // Remove the player from the platform
        if (!_playersOnThisPlatform.Remove(player))
            return;

        // Decrement the elevator platform count
        _elevatorPlatformCount--;
        
        // Remove the parent of the player
        player.transform.SetParent(null);
        
        // If the platform count is less than or equal to 0, reset the player to their original scene.
        player.gameObject.transform.parent = player.OriginalSceneObject.transform;
        player.gameObject.transform.parent = null;

        // Re-enable the player's jumping
        (player.PlayerController as PlayerMovementV2)?.BasicPlayerMovement.RemoveJumpDisabler(this);
    }
}