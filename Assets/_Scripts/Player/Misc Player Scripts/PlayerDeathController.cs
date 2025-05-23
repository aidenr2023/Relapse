﻿using System;
using UnityEngine;

public class PlayerDeathController : ComponentScript<Player>
{
    public PlayerInfoEventVariable OnRespawn;

    #region Private Fields

    private object _deathSender;
    private HealthChangedEventArgs _deathArgs;

    #endregion

    public void Respawn(Vector3 position)
    {
        var movementV2 = ParentComponent.PlayerController as PlayerMovementV2;

        // Respawn at the current checkpoint
        if (movementV2 != null)
        {
            // CheckpointManager.Instance.RespawnAtCurrentCheckpoint(movementV2.Rigidbody);
            CheckpointManager.Instance.RespawnAt(ParentComponent, position);
        }

        // Reset the player's information when they respawn
        ParentComponent.PlayerInfo.ResetPlayer();

        // Invoke the respawn event
        OnRespawn.Value.Invoke(ParentComponent.PlayerInfo);
    }
    
    public void Respawn(Vector3 position, Quaternion rotation)
    {
        var movementV2 = ParentComponent.PlayerController as PlayerMovementV2;

        // Respawn at the current checkpoint
        if (movementV2 != null)
        {
            // CheckpointManager.Instance.RespawnAtCurrentCheckpoint(movementV2.Rigidbody);
            CheckpointManager.Instance.RespawnAt(ParentComponent, position, rotation);
        }

        // Reset the player's information when they respawn
        ParentComponent.PlayerInfo.ResetPlayer();

        // Invoke the respawn event
        OnRespawn.Value.Invoke(ParentComponent.PlayerInfo);
    }
}