using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.SceneManagement;


public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    #region Serialized Fields

    [SerializeField] private Transform initialSpawnPosition;
    [SerializeField] private CheckpointInteractable[] checkpointList;

    #endregion

    #region Private Fields

    private int _mostRecentCheckpointIndex = -1;
    private int _highestCheckpoint = -1;

    #endregion

    #region Getters

    public Transform CurrentRespawnPosition => _highestCheckpoint < 0
        ? initialSpawnPosition
        : checkpointList[_highestCheckpoint].RespawnPosition;

    #endregion

    private void Awake()
    {
        Instance = this;
    }

    private void RespawnAt(GameObject objectToMove, Transform position)
    {
        // Set the position of the player to the position of the checkpoint
        objectToMove.transform.position = position.position;
    }

    // When player interacts with a burner phone, save the current checkpoint as the transform of the burner phone
    public void SaveCheckpoint(CheckpointInteractable interactedObject)
    {
        // Set current checkpoint to that object
        _mostRecentCheckpointIndex = Array.IndexOf(checkpointList, interactedObject);

        // See if the checkpoint is higher than the highest checkpoint
        if (_mostRecentCheckpointIndex > _highestCheckpoint)
            _highestCheckpoint = _mostRecentCheckpointIndex;
    }

    public void RespawnAtCurrentCheckpoint(GameObject objectToMove) =>
        RespawnAt(objectToMove, CurrentRespawnPosition);
}