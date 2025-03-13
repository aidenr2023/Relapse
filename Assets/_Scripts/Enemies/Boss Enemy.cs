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
    private static float _bossHealth;
    private static bool _bossStarted;
    private static int _staticPhase;

    #region Serialized Fields

    [SerializeField] private Slider bossHealthSlider;
    
    [SerializeField] private BossEnemyAttack bossEnemyAttack;
    [SerializeField] private TempShootingEnemyAttack attack1;

    [SerializeField] private BossPhaseInfo[] bossPhases;

    [SerializeField] private UnityEvent onGoodEnding;
    [SerializeField] private UnityEvent onBadEnding;

    #endregion

    #region Private Fields

    private int _currentPhase;

    private IEnemyAttackBehavior _currentAttackBehavior;

    #endregion

    #region Getters

    public int CurrentPhase => _currentPhase;

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

        _currentPhase = -1;

        // Subscribe to the OnDamaged event
        ParentComponent.OnDamaged += ActivatePhaseChange;

        ParentComponent.OnDamaged += ChangeHealthBar;
        ParentComponent.OnHealed += ChangeHealthBar;

        ParentComponent.OnDamaged += ChangeStaticHealth;
        ParentComponent.OnHealed += ChangeStaticHealth;

        ParentComponent.OnDeath += DetermineEndingOnDeath;

        // Set the max boss health
        StaticallyInitialize(this);
    }

    private void ChangeStaticHealth(object sender, HealthChangedEventArgs e)
    {
        // Change the static health
        _bossHealth = ParentComponent.CurrentHealth;
    }

    private static void StaticallyInitialize(BossEnemy bossEnemy)
    {
        if (_bossStarted)
        {
            // Set the health of the boss using the static variables
            bossEnemy.ParentComponent.ForceCurrentHealth(_bossHealth);
            var healthPercentage = bossEnemy.ParentComponent.CurrentHealth / bossEnemy.ParentComponent.MaxHealth;
            bossEnemy.SetHealthBar(healthPercentage);
            Debug.Log($"SETTING BOSS HEALTH TO: {_bossHealth}");

            // Set the current phase of the boss using the static variables
            bossEnemy._currentPhase = _staticPhase;
            
            return;
        }

        _bossStarted = true;
    }

    private void DetermineEndingOnDeath(object sender, HealthChangedEventArgs e)
    {
        // Get the instance of the player.
        // If it has ANY neuros in the powers list, then the player has the bad ending.
        var goodEnding =
            Player.Instance == null &&
            Player.Instance.PlayerPowerManager.Powers.All(n => n.PowerType != PowerType.Drug);

        if (goodEnding)
        {
            Debug.Log("GOOD ENDING");
            onGoodEnding.Invoke();
        }
        else
        {
            Debug.Log("BAD ENDING");
            onBadEnding.Invoke();
        }
    }

    private void ChangeHealthBar(object sender, HealthChangedEventArgs e)
    {
        // TODO: Implement health bar change

        var healthPercentage = ParentComponent.CurrentHealth / ParentComponent.MaxHealth;
        
        // Change the fill of the health bar
        SetHealthBar(healthPercentage);
    }

    private void SetHealthBar(float percent)
    {
        if (bossHealthSlider == null)
            return;
        
        bossHealthSlider.value = percent;
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

        for (var i = bossPhases.Length - 1; i > _currentPhase; i--)
        {
            // If the current phase is completed,
            // then that means the rest are completed. Break
            if (_currentPhase > i)
                break;

            // If the health percent is lower than the phase end percent,
            // Update the current phase and call the phase end event
            if (healthPercent > bossPhases[i].phaseEndPercent)
                continue;

            // NOTE: This only activates the phase end event for the HIGHEST phase that is completed.
            // If the player completes multiple phases in one frame,
            // only the highest phase will be activated.

            _currentPhase = i;
            _staticPhase = i;
            bossPhases[i].phaseEndEvent.Invoke();

            Debug.Log($"Phase {_currentPhase + 1} activated! {healthPercent} <= {bossPhases[i].phaseEndPercent}");
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

    public string GetDebugText()
    {
        StringBuilder sb = new();

        sb.AppendLine($"Boss Enemy: {name}");
        sb.AppendLine($"\tHealth: {(ParentComponent.CurrentHealth / ParentComponent.MaxHealth):0.00}");
        sb.AppendLine($"\tCurrent Phase: {_currentPhase + 1}");

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