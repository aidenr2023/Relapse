using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class StationaryEnemyMovement : MonoBehaviour, INewEnemyMovementBehavior
{
    #region Serialized Fields

    [SerializeField, Range(0, 1)] private float rotationLerpAmount = 0.15f;

    #endregion

    #region Private Fields

    private NavMeshAgent _navMeshAgent;

    #endregion

    #region Getters

    public Enemy Enemy { get; private set; }

    public NewEnemyBehaviorBrain Brain { get; private set; }
    public NewEnemyMovement NewMovement { get; private set; }

    public GameObject GameObject => gameObject;

    public HashSet<object> MovementDisableTokens { get; } = new();
    public TokenManager<float> MovementSpeedTokens { get; } = new(false, null, 1);

    #endregion

    private void Awake()
    {
        // Get the enemy component
        Enemy = GetComponent<Enemy>();

        Brain = GetComponent<NewEnemyBehaviorBrain>();
        NewMovement = GetComponent<NewEnemyMovement>();

        // Get the nav mesh agent component
        _navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        // Set the forward of the transform to the detection target
        if (!Enemy.DetectionBehavior.IsTargetDetected)
            return;

        // Get the current rotation of the forward vector
        var currentRotation = transform.rotation;

        // Get the desired rotation of the forward vector
        var difference = Enemy.DetectionBehavior.LastKnownTargetPosition - transform.position;
        var desiredRotation = Quaternion.LookRotation(difference, Vector3.up);

        // Rotate the forward of the transform towards the target forward
        var newRotation = Quaternion.Lerp(currentRotation, desiredRotation,
            CustomFunctions.FrameAmount(rotationLerpAmount)
        );
        
        // Create a new rotation WITHOUT a rotation around the x or z axis
        var newRotationNoXZ = Quaternion.Euler(0, newRotation.eulerAngles.y, 0);

        // Set the rotation of the transform
        transform.rotation = newRotationNoXZ;
    }

    public void StateUpdateMovement(NewEnemyBehaviorBrain brain, NewEnemyMovement newMovement,
        bool needsToUpdateDestination)
    {
    }
}