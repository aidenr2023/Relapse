using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelCheckpointReset : MonoBehaviour
{
    #region Private Fields

    private readonly Dictionary<Player, Collider> _playersInCollider = new();

    #endregion

    private void OnTriggerEnter(Collider other)
    {
        // Return if the other object is not the player
        if (!other.CompareTag("Player"))
            return;

        // Return if the other object does not have the Player component in its parent
        if (!other.TryGetComponentInParent(out Player player))
            return;

        // Add the player to the hashset
        // Return if another collider of the same player is already in the hashset
        if (!_playersInCollider.TryAdd(player, other))
            return;

        // Reset the player to the checkpoint
        LevelCheckpointManager.Instance.ResetToCheckpoint(LevelCheckpointManager.Instance.CurrentCheckpoint);
    }

    private void OnTriggerExit(Collider other)
    {
        // Return if the other object is not the player
        if (!other.CompareTag("Player"))
            return;

        // Return if the other object does not have the Player component in its parent
        if (!other.TryGetComponentInParent(out Player player))
            return;

        // Return if the collider that is exiting is not the same collider that entered
        if (!_playersInCollider.TryGetValue(player, out var cCollider) || cCollider != other)
            return;

        // Remove the player from the hashset
        _playersInCollider.Remove(player);
    }
}