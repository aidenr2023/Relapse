using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(EnemyInfo))]
public class BossEnemy : ComponentScript<EnemyInfo>, IDebugged
{
    #region Serialized Fields

    [SerializeField] private IntReference bossCurrentPhase;

    [SerializeField] private BossEnemyAttack bossEnemyAttack;
    [SerializeField] private TempShootingEnemyAttack attack1;

    [SerializeField] private BossPhaseInfo[] bossPhases;

    [SerializeField] private UnityEvent onGoodEnding;
    [SerializeField] private UnityEvent onBadEnding;

    #endregion

    #region Private Fields

    private IEnemyAttackBehavior _currentAttackBehavior;

    #endregion

    protected override void CustomAwake()
    {
        // Order the boss phases descending order by their phaseEndPercent
        bossPhases = bossPhases.OrderByDescending(phase => phase.phaseEndPercent).ToArray();
    }

    // Start is called before the first frame update
    private void Start()
    {
        // Switch to the first attack behavior
        ChangeAttackBehavior(attack1);

        // Subscribe to the OnDamaged event
        ParentComponent.OnDamaged += SetHealthScriptableObjects;
        ParentComponent.OnHealed += SetHealthScriptableObjects;

        ParentComponent.OnDamaged += ActivatePhaseChange;

        ParentComponent.OnDeath += DetermineEndingOnDeath;
    }

    private void SetHealthScriptableObjects(object sender, HealthChangedEventArgs e)
    {
    }

    private void DetermineEndingOnDeath(object sender, HealthChangedEventArgs e)
    {
        // Get the relapse count
        var relapseCount = Player.Instance?.PlayerInfo.RelapseCount ?? 0;

        // Get the instance of the player.
        // If the player has relapsed at all, then it's a bad ending.
        if (relapseCount <= 0)
        {
            Debug.Log($"GOOD ENDING: {relapseCount}");
            onGoodEnding.Invoke();
        }
        else
        {
            Debug.Log($"BAD ENDING: {relapseCount}");
            onBadEnding.Invoke();
        }
    }

    private void OnEnable()
    {
        DebugManager.Instance.AddDebuggedObject(this);
    }

    private void OnDisable()
    {
        DebugManager.Instance.RemoveDebuggedObject(this);
    }

    private void ActivatePhaseChange(object sender, HealthChangedEventArgs e)
    {
        var healthPercent = ParentComponent.CurrentHealth / ParentComponent.MaxHealth;

        for (var i = bossPhases.Length - 1; i > bossCurrentPhase.Value; i--)
        {
            // If the current phase is completed,
            // then that means the rest are completed. Break
            if (bossCurrentPhase.Value > i)
                break;

            // If the health percent is lower than the phase end percent,
            // Update the current phase and call the phase end event
            if (healthPercent > bossPhases[i].phaseEndPercent)
                continue;

            // NOTE: This only activates the phase end event for the HIGHEST phase that is completed.
            // If the player completes multiple phases in one frame,
            // only the highest phase will be activated.

            bossCurrentPhase.Value = i;
            bossPhases[i].phaseEndEvent.Invoke();
            
            Debug.Log($"Phase {bossCurrentPhase.Value + 1} activated! {healthPercent} <= {bossPhases[i].phaseEndPercent}");
        }
    }

    private void ChangeAttackBehavior(IEnemyAttackBehavior newBehavior)
    {
        // Create an array of all the attack behaviors
        var attackBehaviors = new IEnemyAttackBehavior[]
        {
            bossEnemyAttack, attack1
        };

        // Iterate through all of them and disable the ones that aren't the current one
        foreach (var cBehavior in attackBehaviors)
        {
            // Skip the current behavior
            if (cBehavior == newBehavior)
                continue;

            // Disable the attack behavior
            (cBehavior as MonoBehaviour)!.enabled = false;
        }

        // Enable the new behavior
        (newBehavior as MonoBehaviour)!.enabled = true;

        // Set the current attack behavior
        _currentAttackBehavior = newBehavior;
    }

    public void MakeInvincible()
    {
        // Prevent the current health from falling below the phase end percent
        ParentComponent.ForceCurrentHealth(
            Mathf.Max(bossPhases[bossCurrentPhase].phaseEndPercent * ParentComponent.MaxHealth, ParentComponent.CurrentHealth)
        );
        
        ParentComponent.AddInvincibilityToken(this);
    }

    public string GetDebugText()
    {
        StringBuilder sb = new();

        sb.AppendLine($"Boss Enemy: {name}");
        sb.AppendLine($"\tHealth: {(ParentComponent.CurrentHealth / ParentComponent.MaxHealth):0.00}");
        sb.AppendLine($"\tCurrent Phase: {bossCurrentPhase.Value + 1}");

        return sb.ToString();
    }

    public enum BossBehavior : byte
    {
        Gun,
        Power,
    }

    [Serializable]
    private struct BossPhaseInfo
    {
        [Range(0, 1)] public float phaseEndPercent;
        public UnityEvent phaseEndEvent;
    }
}