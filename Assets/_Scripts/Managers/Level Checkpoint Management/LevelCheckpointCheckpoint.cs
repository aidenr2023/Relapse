using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class LevelCheckpointCheckpoint : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private Transform respawnPoint;
    [SerializeField] private float rotation;

    #endregion

    #region Private Fields

    private readonly Dictionary<Player, Collider> _playersInCollider = new();

    #endregion

    #region Getters

    public Transform RespawnPoint => respawnPoint;
    public float Rotation => rotation;

    public Vector3 RespawnForward
    {
        get
        {
            var forward = Quaternion.Euler(0, rotation, 0) * respawnPoint.forward;
            forward = new Vector3(forward.x, 0, forward.z).normalized;

            return forward;
        }
    }

    #endregion

    private void SetPlayerCheckpoint(Collider other)
    {
        // Return if the other object is not the player
        if (!other.CompareTag("Player"))
            return;

        // Return if the other object does not have the Player component in its parent
        if (!other.TryGetComponentInParent(out Player player))
            return;

        // Add the player to the hashset
        // Return if another collider of the same player is already in the hashset
        if (!_playersInCollider.TryAdd(player, other))
            return;

        // Set the checkpoint
        LevelCheckpointManager.Instance.SetCheckpoint(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        SetPlayerCheckpoint(other);
    }

    private void OnTriggerExit(Collider other)
    {
        // Return if the other object is not the player
        if (!other.CompareTag("Player"))
            return;

        // Return if the other object does not have the Player component in its parent
        if (!other.TryGetComponentInParent(out Player player))
            return;

        // Return if the collider that is exiting is not the same collider that entered
        if (!_playersInCollider.TryGetValue(player, out var cCollider) || cCollider != other)
            return;

        // Remove the player from the hashset
        _playersInCollider.Remove(player);
    }

    private void OnDrawGizmosSelected()
    {
        // Return if there is no respawn point
        if (respawnPoint == null)
            return;

        // Get the forward of the respawn point + the rotation
        var forward = RespawnForward;

        const float sphereSize = 0.25f;

        // Draw the respawn point
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(respawnPoint.position, sphereSize);

        Gizmos.color = Color.red;
        CustomFunctions.DrawArrow(respawnPoint.position, forward);
        
        // const float arrowLength = 3f;
        // const float arrowYOffset = 2f;
        //
        // var arrowStart = respawnPoint.position - forward * arrowLength / 2 + Vector3.up * arrowYOffset;
        // var arrowEnd = respawnPoint.position + forward * arrowLength + Vector3.up * arrowYOffset;
        //
        // // Draw the forward of the respawn point
        // Gizmos.color = Color.red;
        // Gizmos.DrawLine(arrowStart, arrowEnd);
        //
        // // Draw the arrow head
        // const float arrowAngleSize = 30;
        // Gizmos.DrawLine(
        //     arrowEnd,
        //     arrowEnd + Quaternion.Euler(0, arrowAngleSize, 0) * -forward * arrowLength / 4
        // );
        // Gizmos.DrawLine(
        //     arrowEnd,
        //     arrowEnd + Quaternion.Euler(0, -arrowAngleSize, 0) * -forward * arrowLength / 4
        // );
    }
}