using System;
using UnityEngine;

public interface IEnemyDetectionBehavior : IEnemyBehavior
{
    public event Action<IEnemyDetectionBehavior, EnemyDetectionState, EnemyDetectionState> OnDetectionStateChanged;

    public bool IsDetectionEnabled { get; set; }

    public EnemyDetectionState CurrentDetectionState { get; }

    public bool IsTargetDetected { get; }

    public IActor Target { get; }

    public Vector3 LastKnownTargetPosition { get; }
}