using System;
using UnityEngine;

public class PlayerDeathController : ComponentScript<Player>
{
    #region Serialized Fields

    [SerializeField] private bool canDie = true;

    [SerializeField] private float blackScreenOnTime = 2;

    #endregion

    #region Private Fields

    private CountdownTimer _blackScreenOnTimer;

    private object _deathSender;
    private HealthChangedEventArgs _deathArgs;

    private bool _isAlreadyDead;

    #endregion

    protected override void CustomAwake()
    {
        base.CustomAwake();

        // Initialize the black screen on timer
        _blackScreenOnTimer = new CountdownTimer(blackScreenOnTime);
        _blackScreenOnTimer.OnTimerEnd += LightenScreenAfterDeath;
        _blackScreenOnTimer.OnTimerEnd += () => EnableDeathScreenOnDeath(_deathSender, _deathArgs);
    }

    private void Start()
    {
        // Subscribe to the OnDeath event
        // ParentComponent.PlayerInfo.OnDeath += EnableDeathScreenOnDeath;
        ParentComponent.PlayerInfo.OnDeath += SetDeathInfo;
        ParentComponent.PlayerInfo.OnDeath += DarkenScreenOnDeath;
    }

    private void Update()
    {
        // If the player is not dead, stop
        if (ParentComponent.PlayerInfo.CurrentHealth > 0)
        {
            _isAlreadyDead = false;
            _blackScreenOnTimer.Stop();
        }

        // Update the black screen on timer
        _blackScreenOnTimer.SetMaxTime(blackScreenOnTime);
        _blackScreenOnTimer.Update(Time.unscaledDeltaTime);
    }

    private void DarkenScreenOnDeath(object sender, HealthChangedEventArgs args)
    {
        // If the player cannot die, return
        if (!canDie)
            return;

        // If the player is already dead, return
        if (_isAlreadyDead)
            return;

        // Darken the screen
        BlackScreenOverlayHandler.Instance.DarkenScreen();

        // Start the black screen on timer
        _blackScreenOnTimer.SetMaxTimeAndReset(blackScreenOnTime);
        _blackScreenOnTimer.Start();

        // Set the player as already dead
        _isAlreadyDead = true;
    }

    private void LightenScreenAfterDeath()
    {
        // Stop the black screen on timer
        _blackScreenOnTimer.Stop();

        // If the player cannot die, return
        if (!canDie)
            return;

        // Lighten the screen
        BlackScreenOverlayHandler.Instance.LightenScreen();
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

    private void SetDeathInfo(object sender, HealthChangedEventArgs args)
    {
        _deathSender = sender;
        _deathArgs = args;
    }

    public void Respawn()
    {
        var movementV2 = ParentComponent.PlayerController as PlayerMovementV2;

        // Respawn at the current checkpoint
        if (movementV2 != null)
            CheckpointManager.Instance.RespawnAtCurrentCheckpoint(movementV2.Rigidbody);

        // Reset the player's information when they respawn
        ParentComponent.PlayerInfo.ResetPlayer();
    }
}