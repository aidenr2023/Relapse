using System;
using UnityEngine;

public interface IEnemyDetectionBehavior : IEnemyBehavior
{
    /// <summary>
    /// sender, previous state, new state
    /// </summary>
    public event Action<IEnemyDetectionBehavior, EnemyDetectionState, EnemyDetectionState> OnDetectionStateChanged;

    public bool IsDetectionEnabled { get; }

    public EnemyDetectionState CurrentDetectionState { get; }

    public bool IsTargetDetected { get; }

    public IActor Target { get; }

    public Vector3 LastKnownTargetPosition { get; }

    public void SetDetectionEnabled(bool on);
}