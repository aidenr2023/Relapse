using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(EnemyInfo))]
public class BossEnemy : ComponentScript<EnemyInfo>, IDebugged
{
    #region Serialized Fields

    [SerializeField] private BossPhaseInfo[] bossPhases;

    [SerializeField] private UnityEvent onGoodEnding;
    [SerializeField] private UnityEvent onBadEnding;

    #endregion

    #region Private Fields

    private EnemyInfo _enemyInfo;

    private int _currentPhase;

    #endregion

    #region Getters

    public int CurrentPhase => _currentPhase;

    #endregion

    protected override void CustomAwake()
    {
        // Get the EnemyInfo component
        _enemyInfo = GetComponent<EnemyInfo>();

        // Order the boss phases descending order by their phaseEndPercent
        bossPhases = bossPhases.OrderByDescending(phase => phase.phaseEndPercent).ToArray();
    }

    // Start is called before the first frame update
    private void Start()
    {
        _currentPhase = -1;

        // Subscribe to the OnDamaged event
        _enemyInfo.OnDamaged += ActivatePhaseChange;

        _enemyInfo.OnDamaged += ChangeHealthBar;
        _enemyInfo.OnHealed += ChangeHealthBar;

        _enemyInfo.OnDeath += DetermineEndingOnDeath;
    }

    private void DetermineEndingOnDeath(object sender, HealthChangedEventArgs e)
    {
        // Get the instance of the player.
        // If it has ANY neuros in the powers list, then the player has the bad ending.
        var goodEnding =
            Player.Instance == null && Player.Instance.PlayerPowerManager.Powers.All(n => n.PowerType != PowerType.Drug);

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

        // Change the fill of the health bar
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
        var healthPercent = _enemyInfo.CurrentHealth / _enemyInfo.MaxHealth;

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
            bossPhases[i].phaseEndEvent.Invoke();

            Debug.Log($"Phase {_currentPhase + 1} activated! {healthPercent} <= {bossPhases[i].phaseEndPercent}");
        }
    }

    public string GetDebugText()
    {
        StringBuilder sb = new();

        sb.AppendLine($"Boss Enemy: {name}");
        sb.AppendLine($"\tHealth: {(_enemyInfo.CurrentHealth / _enemyInfo.MaxHealth):0.00}");
        sb.AppendLine($"\tCurrent Phase: {_currentPhase + 1}");

        return sb.ToString();
    }

    [Serializable]
    private struct BossPhaseInfo
    {
        [Range(0, 1)] public float phaseEndPercent;
        public UnityEvent phaseEndEvent;
    }
}