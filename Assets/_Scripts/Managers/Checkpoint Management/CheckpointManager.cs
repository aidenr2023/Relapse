using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.SceneManagement;


public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    #region Serialized Fields

    [SerializeField] private CheckpointInteractable initialSpawnPosition;
    [SerializeField] private CheckpointInteractable[] checkpointList;

    #endregion

    #region Private Fields

    private int _mostRecentCheckpointIndex = -1;
    private int _highestCheckpoint = -1;

    #endregion

    #region Getters

    public CheckpointInteractable CurrentRespawnPoint => _highestCheckpoint < 0
        ? initialSpawnPosition
        : checkpointList[_highestCheckpoint];

    #endregion

    private void Awake()
    {
        Instance = this;
    }

    private void RespawnAt(GameObject objectToMove, CheckpointInteractable checkpoint)
    {
        // // Force the async scene manager to load the checkpoint's scene(s) synchronously before moving the player
        // AsyncSceneManager.Instance.LoadScenesSynchronous(checkpoint.SceneLoaderInformation);

        // Set the position of the player to the position of the checkpoint
        objectToMove.transform.position = checkpoint.RespawnPosition.position;

        // // Wait for the scene to load before moving the player
        // StartCoroutine(WaitThenMove(objectToMove, checkpoint.RespawnPosition.position, 1f));
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
        RespawnAt(objectToMove, CurrentRespawnPoint);

    private IEnumerator WaitThenMove(GameObject objectToMove, Vector3 newPosition, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        objectToMove.transform.position = newPosition;
    }
}