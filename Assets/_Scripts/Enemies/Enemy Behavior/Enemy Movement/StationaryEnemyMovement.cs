using System;
using UnityEngine;

public class StationaryEnemyMovement : MonoBehaviour, IEnemyMovementBehavior
{
    public Enemy Enemy { get; private set; }
    public GameObject GameObject => gameObject;

    public bool IsMovementEnabled => true;


    private void Awake()
    {
        // Get the enemy component
        Enemy = GetComponent<Enemy>();
    }


    public void SetMovementEnabled(bool on)
    {
    }
}