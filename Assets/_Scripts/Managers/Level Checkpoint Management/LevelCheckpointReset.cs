using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelCheckpointReset : MonoBehaviour, IDamager
{
    [SerializeField] private bool damagePlayer = true;

    #region Private Fields

    private readonly Dictionary<Player, Collider> _playersInCollider = new();

    #endregion

    public GameObject GameObject => gameObject;

    protected void OnTriggerEnter(Collider other)
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

        // Call the custom on trigger enter method
        CustomOnTriggerEnter(other, player);
    }

    protected virtual void CustomOnTriggerEnter(Collider other, Player player)
    {
        // Reset the player to the checkpoint
        LevelCheckpointManager.Instance.ResetToCheckpoint(LevelCheckpointManager.Instance.CurrentCheckpoint);

        // Damage the player if the damagePlayer field is true
        if (damagePlayer)
        {
            player.PlayerInfo.ChangeHealth(
                -player.LevelCheckpointDamage,
                null, this,
                player.GameObject.transform.position);
        }
    }

    protected void OnTriggerExit(Collider other)
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