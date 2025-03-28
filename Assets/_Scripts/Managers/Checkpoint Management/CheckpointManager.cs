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
        SaveCheckpoint(interactedObject.RespawnPosition.position);
    }

    public void SaveCheckpoint(Vector3 position)
    {
        var currentSceneInfo = AsyncSceneManager.Instance.CurrentSceneInfo;

        CurrentCheckpointInfo = new CheckpointInformation
        {
            // Get the current level section
            levelSectionSceneInfo = currentSceneInfo,
            position = position
        };

        // Debug.Assert(currentSceneInfo.SectionScene != null, "currentSceneInfo.SectionScene == null");
        // Debug.Log($"Saved Checkpoint: {currentSceneInfo.SectionScene.SceneName} - {CurrentCheckpointInfo.position}");
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