using System;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorPlatform : MonoBehaviour
{
    private readonly HashSet<Player> _playersOnPlatform = new();

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
        if (!_playersOnPlatform.Add(player))
            return;

        // Make this game object the parent of the player
        player.transform.SetParent(transform);
    }

    private void OnTriggerExit(Collider other)
    {
        // If the object does not have a player component, return
        if (!other.TryGetComponent(out Player player))
            return;

        // If the player is not on the platform, return
        // Remove the player from the platform
        if (!_playersOnPlatform.Remove(player))
            return;

        // Remove the parent of the player
        player.transform.SetParent(null);
    }
}