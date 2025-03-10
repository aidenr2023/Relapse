using System;
using System.Collections.Generic;
using UnityEngine;

public class MeleeEnemyAttack : MonoBehaviour, IEnemyAttackBehavior
{
    private static readonly int AnimatorAttackProperty = Animator.StringToHash("Attack");

    #region Serialized Fields

    [SerializeField] [Min(0)] private float damage;

    [SerializeField] [Min(0)] private float meleeAttackRange;

    [SerializeField] [Min(0)] private float attackCooldown;

    [SerializeField] private Animator animator;

    [SerializeField] private MeleeAttackHitbox[] meleeAttackHitboxes;

    #endregion

    #region Private Fields

    private bool _isExternallyEnabled = true;

    private CountdownTimer _attackCooldownTimer;

    // private CountdownTimer _movementDisableTimer;

    private bool _canAttack = true;

    #endregion

    #region Getters

    public Enemy Enemy { get; private set; }
    public GameObject GameObject => gameObject;

    public bool IsAttackEnabled => _isExternallyEnabled && this.IsAttackEnabledTokens();

    public float Damage => damage;

    public HashSet<object> AttackDisableTokens { get; } = new();

    public IReadOnlyList<MeleeAttackHitbox> MeleeAttackHitboxes => meleeAttackHitboxes;
    
    
    [SerializeField] private Sound normalHitSfx;
    
    public Sound NormalHitSfx => normalHitSfx;
    public Sound CriticalHitSfx => null;

    #endregion

    #region Initializiation Functions

    private void Awake()
    {
        // Initialize the components
        InitializeComponents();

        // Initialize the attack cooldown timer
        _attackCooldownTimer = new CountdownTimer(attackCooldown, true, true);

        // Subscribe to the attack cooldown timer's OnTimerEnd event
        _attackCooldownTimer.OnTimerEnd += () => _canAttack = true;

        // // Initialize the movement disable timer
        // _movementDisableTimer = new CountdownTimer(0, false, false);
        //
        // // Subscribe to the movement disable timer's OnTimerEnd event
        // _movementDisableTimer.OnTimerEnd += () =>
        // {
        //     // Re-enable movement
        //     Enemy.EnemyMovementBehavior.SetMovementEnabled(true);
        //
        //     // Stop the timer
        //     _movementDisableTimer.Stop();
        //
        //     // Turn off all the hit boxes
        //     SetAllHitBoxes(false);
        // };
    }

    private void InitializeComponents()
    {
        // Get the enemy component
        Enemy = GetComponent<Enemy>();
    }

    private void Start()
    {
    }

    #endregion

    #region Update Methods

    private void Update()
    {
        // Update the attack cooldown timer
        _attackCooldownTimer.Update(Time.deltaTime);

        // // Update the movement disable timer
        // _movementDisableTimer?.Update(Time.deltaTime);

        // Check if the player is in range
        var isTargetInRange = IsTargetInRange();

        // Return if disabled
        if (!IsAttackEnabled)
            return;

        // If the player is in range and the attack cooldown has expired, attack the player
        if (isTargetInRange && _canAttack)
            Attack();
    }

    private void Attack()
    {
        // Set the can attack flag to false
        _canAttack = false;

        // Reset the attack cooldown timer
        _attackCooldownTimer.SetMaxTimeAndReset(attackCooldown);
        _attackCooldownTimer.Start();

        if (animator == null)
            return;

        // Play the melee attack animation
        animator.SetTrigger(AnimatorAttackProperty);
    }

    private bool IsTargetInRange()
    {
        // Return if there is no target
        if (Enemy.DetectionBehavior?.Target == null)
            return false;

        // If the enemy is not aware of the target, return false
        if (Enemy.DetectionBehavior.CurrentDetectionState != EnemyDetectionState.Aware)
            return false;

        var distance = Vector3.Distance(transform.position,
            Enemy.DetectionBehavior.Target.GameObject.transform.position);

        // Check if the player is in range
        return distance <= meleeAttackRange;
    }

    #endregion

    public void SetAttackEnabled(bool on)
    {
        _isExternallyEnabled = on;
    }

    public void ActivateHitBox(int index)
    {
        if (meleeAttackHitboxes == null || index < 0 || index >= meleeAttackHitboxes.Length)
            return;

        // Debug.Log($"Activating Hit Box {index}");

        meleeAttackHitboxes[index]?.SetEnabled(true);
        
        // Play the c
        meleeAttackHitboxes[index]?.PlayTrail();
    }

    public void DeactivateHitBox(int index)
    {
        if (meleeAttackHitboxes == null || index < 0 || index >= meleeAttackHitboxes.Length)
            return;

        meleeAttackHitboxes[index]?.SetEnabled(false);
        
        // Stop the trail
        meleeAttackHitboxes[index]?.StopTrail();
    }

    #region Debugging

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeAttackRange);
    }

    #endregion
}