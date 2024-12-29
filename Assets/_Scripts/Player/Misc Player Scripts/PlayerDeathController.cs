using System;
using UnityEngine;

public class PlayerDeathController : ComponentScript<Player>
{
    [SerializeField] private bool canDie = true;

    private void Start()
    {
        // Subscribe to the OnDeath event
        ParentComponent.PlayerInfo.OnDeath += EnableDeathScreenOnDeath;
    }

    private void EnableDeathScreenOnDeath(object sender, HealthChangedEventArgs args)
    {
        // If the player cannot die, return
        if (!canDie)
            return;

        var isRelapse = args.DamagerObject == args.Actor;

        // Enable the Relapse Screen when the player relapses or there is no checkpoint manager
        if (isRelapse)
            RelapseScreen.Instance.InitializeRelapse();
        else
            RelapseScreen.Instance.InitializeDeath();

        // Activate the Relapse Screen
        RelapseScreen.Instance.Activate();
    }

    public void Respawn()
    {
        var movementV2 = ParentComponent.PlayerController as PlayerMovementV2;

        // Respawn at the current checkpoint
        CheckpointManager.Instance.RespawnAtCurrentCheckpoint(movementV2.Rigidbody);

        // Reset the player's information when they respawn
        ParentComponent.PlayerInfo.ResetPlayer();
    }
}