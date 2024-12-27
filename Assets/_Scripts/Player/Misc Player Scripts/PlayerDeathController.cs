using System;
using UnityEngine;

public class PlayerDeathController : ComponentScript<Player>
{
    [SerializeField] private bool canDie = true;

    private void Start()
    {
        // Subscribe to the OnDeath event
        ParentComponent.PlayerInfo.OnDeath += OnDeath;
    }

    private void OnDeath(object sender, HealthChangedEventArgs args)
    {
        // If the player cannot die, return
        if (!canDie)
            return;

        var checkpointManager = CheckpointManager.Instance;

        // Enable the Relapse Screen when the player relapses or there is no checkpoint manager
        if (args.DamagerObject == args.Actor || checkpointManager == null)
        {
            RelapseScreen.Instance.Activate();
            return;
        }

        // Respawn the player at the respawn position
        Respawn();

        // Reset the player's information when they respawn
        ParentComponent.PlayerInfo.ResetPlayer();
    }

    private void Respawn()
    {
        // Respawn at the current checkpoint
        CheckpointManager.Instance.RespawnAtCurrentCheckpoint(gameObject);
    }
}