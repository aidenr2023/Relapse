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

    public void RespawnAt(Player player, Vector3 position)
    {
        // Set the position of the player to the position of the checkpoint
        player.Rigidbody.position = position;
    }
    
    public void RespawnAt(Player player, Vector3 position, Quaternion rotation)
    {
        RespawnAt(player, position);
        
        // Set the rotation of the player to the rotation of the checkpoint
        player.PlayerLook.ApplyRotation(rotation);
    }

    // When player interacts with a burner phone, save the current checkpoint as the transform of the burner phone
    public IResult SaveCheckpoint(CheckpointInteractable interactedObject)
    {
        // var positionOption = interactedObject.RespawnPosition != null
        //     ? Option<Vector3>.Some(interactedObject.RespawnPosition.position)
        //     : Option<Vector3>.None;
        //
        // if (!positionOption.HasValue)
        // {
        //     Debug.LogError("CheckpointInteractable does not have a respawn position! Not saving!", interactedObject);
        //     return;
        // }
        //
        // SaveCheckpoint(positionOption.Value);

        // Save the checkpoint if possible 
        var result = interactedObject
            .NullCheckToResult()
            .Map(n => n.RespawnPosition)
            .Check(CustomFunctions.IsNotNull, "interactedObject.RespawnPosition is null!")
            .Map(n => n.position)
            .Chain(n => SaveCheckpoint(n, Player.Instance.PlayerController.CameraPivot.transform.rotation));

        if (result.IsFailure)
            Debug.LogError($"Could not save checkpoint!: {result.ErrorMessage}");

        return result;
    }

    public void SaveCheckpoint(Vector3 position)
    {
        var currentSceneInfo = AsyncSceneManager.Instance.CurrentSceneInfo;

        CurrentCheckpointInfo = new CheckpointInformation
        {
            // Get the current level section
            levelSectionSceneInfo = currentSceneInfo,
            position = position,
            rotation = Quaternion.identity
        };
    }

    public void SaveCheckpoint(Vector3 position, Quaternion rotation)
    {
        var currentSceneInfo = AsyncSceneManager.Instance.CurrentSceneInfo;

        CurrentCheckpointInfo = new CheckpointInformation
        {
            // Get the current level section
            levelSectionSceneInfo = currentSceneInfo,
            position = position,
            rotation = rotation
        };
    }

    // public void RespawnAtCurrentCheckpoint(Rigidbody rb) =>
    //     RespawnAt(rb, CurrentRespawnPoint);

    // private void RespawnAtCurrentCheckpoint(Rigidbody rb)
    // {
    //     RespawnAt(rb, CurrentCheckpointInfo.position);
    // }

    public struct CheckpointInformation
    {
        public LevelSectionSceneInfo levelSectionSceneInfo;
        public Vector3 position;
        public Quaternion rotation;
    }
}