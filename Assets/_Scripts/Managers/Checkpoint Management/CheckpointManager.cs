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

    #endregion

    #region Private Fields

    #endregion

    #region Getters

    public CheckpointInformation CurrentCheckpointInfo { get; private set; }

    #endregion

    private void Awake()
    {
        Instance = this;
    }

    private void RespawnAt(Rigidbody rb, Vector3 position)
    {
        // Set the position of the player to the position of the checkpoint
        rb.position = position;
    }

    // When player interacts with a burner phone, save the current checkpoint as the transform of the burner phone
    public void SaveCheckpoint(CheckpointInteractable interactedObject)
    {
        CurrentCheckpointInfo = new CheckpointInformation
        {
            // Get the current level section
            levelSectionSceneInfo = AsyncSceneManager.Instance.CurrentSceneInfo,
            position = interactedObject.RespawnPosition.position
        };
        
        Debug.Log($"Saved Checkpoint: {CurrentCheckpointInfo.levelSectionSceneInfo.SectionScene.SceneName} - {CurrentCheckpointInfo.position}");
    }

    // public void RespawnAtCurrentCheckpoint(Rigidbody rb) =>
    //     RespawnAt(rb, CurrentRespawnPoint);

    public void RespawnAtCurrentCheckpoint(Rigidbody rb)
    {
        RespawnAt(rb, CurrentCheckpointInfo.position);
    }

    public struct CheckpointInformation
    {
        public LevelSectionSceneInfo levelSectionSceneInfo;
        public Vector3 position;
    }
}