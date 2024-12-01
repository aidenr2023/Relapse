using System;
using UnityEngine;

public class MeleeEnemyAttack : MonoBehaviour, IEnemyAttackBehavior
{
    private static readonly int animatiorAttackProperty = Animator.StringToHash("Attack");

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

    private CountdownTimer _movementDisableTimer;

    private bool _canAttack = true;

    #endregion

    #region Getters

    public Enemy Enemy { get; private set; }
    public GameObject GameObject => gameObject;

    public bool IsAttackEnabled => _isExternallyEnabled;

    public float Damage => damage;

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

        // Initialize the movement disable timer
        _movementDisableTimer = new CountdownTimer(0, false, false);

        // Subscribe to the movement disable timer's OnTimerEnd event
        _movementDisableTimer.OnTimerEnd += () =>
        {
            // Re-enable movement
            Enemy.EnemyMovementBehavior.SetMovementEnabled(true);

            // Stop the timer
            _movementDisableTimer.Stop();

            // Turn off all the hit boxes
            SetAllHitBoxes(false);
        };
        
    }

    private void InitializeComponents()
    {
        // Get the enemy component
        Enemy = GetComponent<Enemy>();
    }

    private void Start()
    {
        Enemy.EnemyInfo.OnDamaged += (_, args) =>
        {
            //debug log to check if hit trigger is activated
            Debug.Log("Hit Trigger Activated");
            // Activate the animator's hit trigger
            animator.SetTrigger("Hit");
            
        };
    }

    #endregion

    #region Update Methods

    private void Update()
    {
        // Update the attack cooldown timer
        _attackCooldownTimer.Update(Time.deltaTime);

        // Update the movement disable timer
        _movementDisableTimer?.Update(Time.deltaTime);

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
        animator.SetTrigger(animatiorAttackProperty);

        var animationInfo = animator.GetCurrentAnimatorStateInfo(0);

        // Get the length of the attack animation
        var animationLength = animationInfo.length;

        // Reset the movement disable timer
        _movementDisableTimer.SetMaxTimeAndReset(animationLength * animationInfo.speed);
        _movementDisableTimer.Start();

        // Disable movement
        Enemy.EnemyMovementBehavior.SetMovementEnabled(false);

        // Turn on all the hit boxes
        SetAllHitBoxes(true);
    }

    private bool IsTargetInRange()
    {
        // Return if there is no target
        if (Enemy.EnemyDetectionBehavior?.Target == null)
            return false;

        // If the enemy is not aware of the target, return false
        if (Enemy.EnemyDetectionBehavior.CurrentDetectionState != EnemyDetectionState.Aware)
            return false;

        var distance = Vector3.Distance(transform.position,
            Enemy.EnemyDetectionBehavior.Target.GameObject.transform.position);

        // Check if the player is in range
        return distance <= meleeAttackRange;
    }

    private void SetAllHitBoxes(bool on)
    {
        Debug.Log($"Setting HitBox colliders to {on}");

        foreach (var hitBox in meleeAttackHitboxes)
            hitBox.SetEnabled(on);
    }

    #endregion

    public void SetAttackEnabled(bool on)
    {
        _isExternallyEnabled = on;
    }

    #region Debugging

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeAttackRange);
    }

    #endregion
}