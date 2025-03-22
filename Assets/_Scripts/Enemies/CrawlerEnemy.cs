using System;
using System.Collections;
using UnityEngine;

public class CrawlerEnemy : ComponentScript<EnemyInfo>
{
    private static readonly int AttackModeAnimationID = Animator.StringToHash("AttackMode");

    #region Serialized Fields

    [SerializeField] private Animator animator;

    [SerializeField] private CrawlerAttackBehaviorInfo<ShootingEnemyAttack> shootingEnemyAttack;
    [SerializeField] private CrawlerAttackBehaviorInfo<MeleeEnemyAttack> meleeEnemyAttack;

    #endregion

    #region Private Fields

    private CrawlerAttackMode _currentAttackMode;

    private Coroutine _updateCoroutine;
    private Coroutine _currentAttackCoroutine;

    #endregion

    private ICrawlerAttackBehavior[] EnemyAttackBehaviors => new ICrawlerAttackBehavior[]
    {
        shootingEnemyAttack,
        meleeEnemyAttack
    };

    private void Start()
    {
        // Change the attack behavior to the shooting
        ChangeEnemyAttack(GetRandomBehaviorMode());
    }

    private void OnEnable()
    {
        // If there is an update coroutine, stop it
        if (_updateCoroutine != null)
            StopCoroutine(_updateCoroutine);

        _updateCoroutine = StartCoroutine(UpdateCoroutine());
    }

    private void OnDisable()
    {
        // If there is an update coroutine, stop it
        if (_updateCoroutine != null)
        {
            StopCoroutine(_updateCoroutine);
            _updateCoroutine = null;
        }
    }

    private void ChangeEnemyAttack(CrawlerAttackMode attackMode)
    {
        // Set the current attack mode to the attack mode
        _currentAttackMode = attackMode;

        switch (attackMode)
        {
            case CrawlerAttackMode.Shooting:
                ChangeEnemyAttack(shootingEnemyAttack);
                break;

            case CrawlerAttackMode.Melee:
                ChangeEnemyAttack(meleeEnemyAttack);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(attackMode), attackMode, null);
        }

        // Change the behavior mode of the brain
        ParentComponent.ParentComponent.Brain.BehaviorMode = (int)attackMode;

        Debug.Log($"Changed attack mode to {attackMode} ({(int)attackMode})", this);

        // Set the animator's attack mode integer to the attack mode
        animator.SetInteger(AttackModeAnimationID, (int)attackMode);
    }

    private void ChangeEnemyAttack(ICrawlerAttackBehavior enemyAttackBehavior)
    {
        // Get a copy of the enemy attack behaviors
        var enemyAttackBehaviors = EnemyAttackBehaviors;

        // For each attack behavior in the enemy attack behaviors
        foreach (var behavior in enemyAttackBehaviors)
        {
            // Skip the enemy attack behavior
            if (behavior == enemyAttackBehavior)
                continue;

            // Disable the attack behavior
            behavior.AttackBehavior.SetAttackEnabled(false);
        }

        // Enable the attack behavior
        enemyAttackBehavior.AttackBehavior.SetAttackEnabled(true);

        // Change the attack behavior to the enemy attack behavior
        ParentComponent.ParentComponent.AttackBehavior = enemyAttackBehavior.AttackBehavior;
    }

    private ICrawlerAttackBehavior GetAttackBehavior(CrawlerAttackMode attackMode)
    {
        switch (attackMode)
        {
            case CrawlerAttackMode.Shooting:
                return shootingEnemyAttack;

            case CrawlerAttackMode.Melee:
                return meleeEnemyAttack;

            default:
                throw new ArgumentOutOfRangeException(nameof(attackMode), attackMode, null);
        }
    }

    private CrawlerAttackMode GetRandomBehaviorMode()
    {
        var num = UnityEngine.Random.Range(0, EnemyAttackBehaviors.Length);

        return (CrawlerAttackMode)num;
    }

    private IEnumerator UpdateCoroutine()
    {
        while (gameObject != null)
        {
            // Start the attack update coroutine for the current attack mode
            _currentAttackCoroutine = StartCoroutine(AttackUpdateCoroutine(_currentAttackMode));

            // Wait for the attack update coroutine to finish
            yield return _currentAttackCoroutine;

            // // Get a random behavior
            // var nextBehavior = GetRandomBehaviorMode();

            // Get the index of the current attack mode
            var currentBehaviorIndex = (int)_currentAttackMode;
            var nextBehaviorIndex = (currentBehaviorIndex + 1) % EnemyAttackBehaviors.Length;

            // Get the next behavior
            var nextBehavior = (CrawlerAttackMode)nextBehaviorIndex;

            // Change the enemy attack behavior to the random behavior
            ChangeEnemyAttack(nextBehavior);
        }
    }

    private IEnumerator AttackUpdateCoroutine(CrawlerAttackMode attackMode)
    {
        // Change the enemy attack behavior to the current attack behavior
        var attackBehavior = GetAttackBehavior(attackMode);

        // Determine how long the enemy will stay in the current attack behavior
        var behaviorTime = UnityEngine.Random.Range(attackBehavior.MinTime, attackBehavior.MaxTime);

        yield return new WaitForSeconds(behaviorTime);
    }

    #region Extra Types

    private interface ICrawlerAttackBehavior
    {
        public IEnemyAttackBehavior AttackBehavior { get; }
        public float MinTime { get; }
        public float MaxTime { get; }
    }

    [Serializable]
    private struct CrawlerAttackBehaviorInfo<TAttackType> : ICrawlerAttackBehavior
        where TAttackType : IEnemyAttackBehavior
    {
        [SerializeField] public TAttackType attackBehavior;
        [field: SerializeField, Min(0)] public float MinTime { get; set; }
        [field: SerializeField, Min(0)] public float MaxTime { get; set; }

        public IEnemyAttackBehavior AttackBehavior => attackBehavior;
    }

    private enum CrawlerAttackMode
    {
        Shooting,
        Melee
    }

    #endregion
}