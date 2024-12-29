using System;
using System.Linq;
using UnityEngine;

public class KillCollider : MonoBehaviour
{
    private void Awake()
    {
        // Assert that there is a trigger collider
        var colliders = GetComponents<Collider>();
        Debug.Assert(colliders.Any(n => n.isTrigger), "No collider found on object");
    }

    private void OnTriggerEnter(Collider other)
    {
        // Return if the other object does not have a player script
        if (!other.TryGetComponent(out Player player))
            return;

        // Kill the player
        player.PlayerInfo.ChangeHealth(
            -player.PlayerInfo.MaxHealth, player.PlayerInfo, player.PlayerInfo,
            other.transform.position
        );
    }
}