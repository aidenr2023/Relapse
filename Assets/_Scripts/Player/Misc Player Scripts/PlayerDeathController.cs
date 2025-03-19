using System;
using UnityEngine;

public class PlayerDeathController : ComponentScript<Player>
{
    #region Serialized Fields

    [SerializeField] private bool canDie = true;
    [SerializeField] private float blackScreenOnTime = 2;

    #endregion

    #region Private Fields

    private object _deathSender;
    private HealthChangedEventArgs _deathArgs;

    #endregion

    public Action<PlayerDeathController> onRespawn;

    public void Respawn()
    {
        var movementV2 = ParentComponent.PlayerController as PlayerMovementV2;

        // Respawn at the current checkpoint
        if (movementV2 != null)
        {
            CheckpointManager.Instance.RespawnAtCurrentCheckpoint(movementV2.Rigidbody);
        }

        // Reset the player's information when they respawn
        ParentComponent.PlayerInfo.ResetPlayer();

        // Invoke the onRespawn event
        onRespawn?.Invoke(this);
    }
}