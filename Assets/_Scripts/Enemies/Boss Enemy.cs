using System;
using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(EnemyInfo))]
public class BossEnemy : ComponentScript<EnemyInfo>, IDebugged
{
    #region Serialized Fields

    [Header("Vars"), SerializeField] private IntReference bossCurrentPhase;
    [SerializeField] private IntReference playerRelapseCount;

    [Header("Attacking"), SerializeField] private BossEnemyAttack bossEnemyAttack;

    [SerializeField] private BossPhaseInfo[] bossPhases;

    [SerializeField] private UnityEvent onGoodEnding;
    [SerializeField] private UnityEvent onBadEnding;

    [SerializeField] private MultipleWorldDialogueTrigger dialogueTrigger;

    #endregion

    #region Private Fields

    /*
     * Have different scripts entirely for the different powers.
     * Also, have an enum for the different attack types (gun + powers).
     * Change the movement type according to the attack type.
     */

    #endregion

    public BossBehaviorMode BossBehaviorMode { get; private set; }
    
    protected override void CustomAwake()
    {
        // Order the boss phases descending order by their phaseEndPercent
        bossPhases = bossPhases.OrderByDescending(phase => phase.phaseEndPercent).ToArray();
    }

    // Start is called before the first frame update
    private void Start()
    {
        // Subscribe to the OnDamaged event
        ParentComponent.OnDamaged += ActivatePhaseChange;
        ParentComponent.OnDeath += DetermineEndingOnDeath;
    }

    private void DetermineEndingOnDeath(object sender, HealthChangedEventArgs e)
    {
        // Get the relapse count
        var relapseCount = playerRelapseCount ?? 0;

        // Get the instance of the player.
        // If the player has relapsed at all, then it's a bad ending.
        if (relapseCount <= 0)
            onGoodEnding.Invoke();
        else
            onBadEnding.Invoke();
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

            Debug.Log(
                $"Phase {bossCurrentPhase.Value + 1} activated! {healthPercent} <= {bossPhases[i].phaseEndPercent}");
        }
    }

    public void SetBossBehaviorMode(BossBehaviorMode mode)
    {
        BossBehaviorMode = mode;
        
        // Change the behavior of the brain
        ParentComponent.ParentComponent.Brain.BehaviorMode = (int) mode;
        
        Debug.Log($"Boss Behavior Mode: {mode} ({(int) mode})", this);
    }
    
    public void MakeInvincible()
    {
        // Prevent the current health from falling below the phase end percent
        ParentComponent.ForceCurrentHealth(
            Mathf.Max(bossPhases[bossCurrentPhase].phaseEndPercent * ParentComponent.MaxHealth,
                ParentComponent.CurrentHealth)
        );

        ParentComponent.AddInvincibilityToken(this);
    }

    public void BadEndingPhaseCheck()
    {
        // if the player has relapsed,
        // then the boss will go into a bad ending phase
        if (playerRelapseCount.Value <= 0)
        {
            Debug.Log("Player has not relapsed. No bad ending phase.");
            return;
        }

        Debug.Log("Player has relapsed. Bad ending phase activated.");

        // Force start the dialogue trigger
        if (dialogueTrigger != null)
            dialogueTrigger.ForceStart();

        // Add an invincibility token
        MakeInvincible();
    }

    public string GetDebugText()
    {
        StringBuilder sb = new();

        sb.AppendLine($"Boss Enemy: {name}");
        sb.AppendLine($"\tHealth: {(ParentComponent.CurrentHealth / ParentComponent.MaxHealth):0.00}");
        sb.AppendLine($"\tCurrent Phase: {bossCurrentPhase.Value + 1}");
        sb.AppendLine($"\tPlayer Relapse Count: {playerRelapseCount.Value}");
        sb.AppendLine($"\tIs Invincible: {ParentComponent.IsInvincible}");

        return sb.ToString();
    }

    [Serializable]
    private struct BossPhaseInfo
    {
        [Range(0, 1)] public float phaseEndPercent;
        public UnityEvent phaseEndEvent;
    }
    
}