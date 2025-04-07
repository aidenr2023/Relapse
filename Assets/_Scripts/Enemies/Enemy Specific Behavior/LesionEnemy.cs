using System;
using System.Collections;
using UnityEngine;

public class LesionEnemy : ComponentScript<EnemyInfo>
{
    #region Serialized Fields

    [SerializeField] private MeleeEnemyAttack meleeEnemyAttack;

    [SerializeField, Min(0)] private float minRelocateTime = 2f;
    [SerializeField, Min(0)] private float maxRelocateTime = 3f;

    [Space, SerializeField, Min(0)] private float minAttackTime = 5f;
    [SerializeField, Min(0)] private float maxAttackTime = 8f;
    [SerializeField] private float attackCountBeforeRetreat = 3;
    [SerializeField, Min(0)] private float attackModeRange = 8f;

    [Space, SerializeField, Min(0)] private float minRetreatTime = 1f;
    [SerializeField, Min(0)] private float maxRetreatTime = 3f;

    #endregion

    #region Private Fields

    private LesionBehaviorMode _currentBehaviorMode;
    private float _currentAttackCount;

    private Coroutine _updateCoroutine;

    #endregion

    protected override void CustomAwake()
    {
        base.CustomAwake();

        // Connect to the attack event of the melee script
        meleeEnemyAttack.OnAttack += IncrementAttackCountOnAttack;
    }

    private void IncrementAttackCountOnAttack(MeleeEnemyAttack obj)
    {
        _currentAttackCount++;
    }

    private void Start()
    {
        ChangeBehaviorMode(LesionBehaviorMode.Relocate);
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

    private IEnumerator UpdateCoroutine()
    {
        yield return null;
        yield return null;

        while (gameObject != null)
        {
            var isWithinAttackRange = false;
            var attackCountSatisfied = false;

            switch (_currentBehaviorMode)
            {
                case LesionBehaviorMode.Relocate:
                    var relocateTime = UnityEngine.Random.Range(minRelocateTime, maxRelocateTime);
                    var relocateEndTime = Time.time + relocateTime;

                    // Wait until the relocate time is over
                    while (Time.time < relocateEndTime)
                    {
                        isWithinAttackRange =
                            ParentComponent.ParentComponent.Brain.DistanceFromTarget <= attackModeRange;

                        // Break if the target is within the melee attack range
                        if (isWithinAttackRange)
                            break;

                        // Wait until the relocate time is over
                        yield return null;
                    }

                    // Change to attack mode
                    ChangeBehaviorMode(LesionBehaviorMode.Attack);
                    break;

                case LesionBehaviorMode.Attack:

                    var attackTime = UnityEngine.Random.Range(minAttackTime, maxAttackTime);
                    var attackEndTime = Time.time + attackTime;

                    while (Time.time < attackEndTime)
                    {
                        attackCountSatisfied = _currentAttackCount >= attackCountBeforeRetreat;

                        // Break if the current attack count is greater than or equal to the attack count before retreat
                        if (attackCountSatisfied)
                            break;

                        // Wait until the attack time is over
                        yield return null;
                    }

                    // If the attack count is NOT satisfied, go back into relocate mode
                    if (!attackCountSatisfied)
                        ChangeBehaviorMode(LesionBehaviorMode.Relocate);

                    // Otherwise, go into retreat mode
                    else
                        ChangeBehaviorMode(LesionBehaviorMode.Retreat);


                    break;

                case LesionBehaviorMode.Retreat:

                    var retreatTime = UnityEngine.Random.Range(minRetreatTime, maxRetreatTime);
                    var retreatEndTime = Time.time + retreatTime;

                    yield return new WaitForSeconds(retreatTime);

                    isWithinAttackRange =
                        ParentComponent.ParentComponent.Brain.DistanceFromTarget <= attackModeRange;

                    // If the player is no longer within attack range, go back into relocate mode
                    if (!isWithinAttackRange)
                        ChangeBehaviorMode(LesionBehaviorMode.Relocate);

                    // Otherwise, go back into attack mode
                    else
                        ChangeBehaviorMode(LesionBehaviorMode.Attack);

                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Wait for the next frame
            yield return null;
        }
    }

    private void ChangeBehaviorMode(LesionBehaviorMode behaviorMode)
    {
        _currentBehaviorMode = behaviorMode;


        switch (_currentBehaviorMode)
        {
            case LesionBehaviorMode.Relocate:
                meleeEnemyAttack.SetAttackEnabled(true);
                break;

            case LesionBehaviorMode.Attack:
                meleeEnemyAttack.SetAttackEnabled(true);

                // Reset the attack count
                _currentAttackCount = 0;

                break;

            case LesionBehaviorMode.Retreat:
                meleeEnemyAttack.SetAttackEnabled(false);
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        // Force the movement update
        ParentComponent.ParentComponent.Brain.ForceMovementUpdate = true;

        // In the brain, change the behavior mode
        ParentComponent.ParentComponent.Brain.BehaviorMode = (int)behaviorMode;

        // Debug.Log($"Lesion Behavior Mode: {behaviorMode} {ParentComponent.ParentComponent.Brain.BehaviorMode}", this);
    }

    #region Extra Types

    private enum LesionBehaviorMode
    {
        Relocate,
        Attack,
        Retreat
    }

    #endregion
}