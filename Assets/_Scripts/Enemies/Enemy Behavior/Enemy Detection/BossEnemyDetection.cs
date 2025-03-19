using System;
using UnityEngine;

public class BossEnemyDetection : MonoBehaviour, IEnemyDetectionBehavior
{
    [SerializeField] private TransformReference targetTransform;

    public event Action<IEnemyDetectionBehavior, EnemyDetectionState, EnemyDetectionState> OnDetectionStateChanged;

    #region Getters

    public GameObject GameObject => gameObject;

    public Enemy Enemy { get; private set; }

    public bool IsDetectionEnabled => true;

    public EnemyDetectionState CurrentDetectionState =>
        IsTargetDetected ? EnemyDetectionState.Aware : EnemyDetectionState.Unaware;

    public bool IsTargetDetected => targetTransform?.Value != null;

    public IActor Target => Player.Instance?.PlayerInfo;

    public Vector3 LastKnownTargetPosition => targetTransform?.Value?.position ?? Vector3.zero;

    public Transform DetectionOrigin => transform;

    #endregion

    private void Awake()
    {
        // Get the enemy component
        Enemy = GetComponent<Enemy>();
    }

    public void SetDetectionEnabled(bool on)
    {
    }
}