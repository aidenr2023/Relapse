using System;
using UnityEngine;

public class OverdriveEnemyAbility : ComponentScript<Enemy>, IEnemyAbilityBehavior
{
    #region Serialized Fields

    [Header("Stats")] [SerializeField] [Min(0)]
    private float overdriveDuration = 3;

    [SerializeField] [Min(0)] private float speedMultiplier = 2;
    [SerializeField] [Min(0)] private float overdriveCooldown = 10;

    // Once every X couple of seconds, check if the enemy should go into overdrive.
    // X is a random number between minRandomCheckTime and maxRandomCheckTime.
    [Header("Overdrive Activation")] [SerializeField] [Range(0, 1)]
    private float overdriveChance;

    [SerializeField] [Min(0)] private float minRandomCheckTime = 3;
    [SerializeField] [Min(0)] private float maxRandomCheckTime = 5;

    #endregion

    #region Private Fields

    private CountdownTimer _overdriveTimer;

    private CountdownTimer _overdriveCheckTimer;

    private CountdownTimer _overdriveCooldownTimer;

    /// <summary>
    /// The speed token that is currently active in the movement behavior's token manager.
    /// </summary>
    private TokenManager<float>.ManagedToken _speedToken;

    #endregion

    #region Getters

    public Enemy Enemy => ParentComponent;
    public GameObject GameObject => gameObject;

    private bool IsOverdriveActive => _overdriveTimer != null && _overdriveTimer.Percentage < 1;

    private bool IsCooldownActive => _overdriveCooldownTimer != null && _overdriveCooldownTimer.Percentage < 1;

    #endregion

    protected override void CustomAwake()
    {
        base.CustomAwake();

        // Initialize the components
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        // Set up the overdrive check timer
        _overdriveCheckTimer = new CountdownTimer(UnityEngine.Random.Range(minRandomCheckTime, maxRandomCheckTime));

        // Add the on timer end event
        _overdriveCheckTimer.OnTimerEnd += () =>
        {
            // Set a random time for the next check
            var randomTime = UnityEngine.Random.Range(minRandomCheckTime, maxRandomCheckTime);
            _overdriveCheckTimer.SetMaxTimeAndReset(randomTime);

            // If the overdrive is already active or the cooldown is active, return
            if (IsOverdriveActive || IsCooldownActive)
                return;

            // If the enemy is not aware of the player, return
            if (Enemy.EnemyDetectionBehavior.CurrentDetectionState != EnemyDetectionState.Aware)
                return;

            // If the random number is less than the overdrive chance, start the overdrive
            if (UnityEngine.Random.value < overdriveChance)
                StartOverdrive();
        };

        // Start the overdrive check timer
        _overdriveCheckTimer.Start();
    }

    private void Update()
    {
        // If the overdrive timer is not null, update the timer
        _overdriveTimer?.Update(Time.deltaTime);

        // If the overdrive cooldown timer is not null, update the timer
        _overdriveCooldownTimer?.Update(Time.deltaTime);

        // If the overdrive check timer is not null, update the timer
        _overdriveCheckTimer?.Update(Time.deltaTime);
    }

    private void StartOverdrive()
    {
        // If there is already an active token, return
        if (_speedToken != null)
            return;

        // Create a new timer for the overdrive duration
        _overdriveTimer = new CountdownTimer(overdriveDuration);

        // Create a new speed token
        _speedToken = Enemy.EnemyMovementBehavior.MovementSpeedTokens.AddToken(speedMultiplier, -1, true);

        // Add the on timer end event
        _overdriveTimer.OnTimerEnd += StopOverdrive;

        // Start the overdrive timer
        _overdriveTimer.Start();

        Debug.Log($"{gameObject.name} is in overdrive!");
    }

    private void StopOverdrive()
    {
        // Remove the speed token
        Enemy.EnemyMovementBehavior.MovementSpeedTokens.RemoveToken(_speedToken);

        // Set the speed token to null
        _speedToken = null;

        // Set the overdrive timer to null
        _overdriveTimer = null;

        // Start the overdrive cooldown
        _overdriveCooldownTimer = new CountdownTimer(overdriveCooldown);
        _overdriveCooldownTimer.Start();

        Debug.Log($"{gameObject.name} is out of overdrive!");
    }
}