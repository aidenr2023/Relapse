using System;
using UnityEngine;

public class MeleeEnemyAttack : MonoBehaviour, IEnemyAttackBehavior
{
    #region Serialized Fields

    [SerializeField] [Min(0)] private float damage;

    #endregion

    #region Private Fields

    private bool _isExternallyEnabled = true;

    #endregion

    #region Getters

    public Enemy Enemy { get; private set; }
    public GameObject GameObject => gameObject;

    public bool IsAttackEnabled => _isExternallyEnabled;

    #endregion

    #region Initializiation Functions

    private void Awake()
    {
        // Initialize the components
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        // Get the enemy component
        Enemy = GetComponent<Enemy>();
    }

    private void Start()
    {
    }

    #endregion

    #region Update Methods

    private void Update()
    {
    }

    #endregion


    public void SetAttackEnabled(bool on)
    {
        _isExternallyEnabled = on;
    }
}