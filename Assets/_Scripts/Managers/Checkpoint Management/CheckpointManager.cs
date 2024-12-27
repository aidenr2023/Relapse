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

    private int _currentCheckpoint;
    private int _highestCheckpoint = -1;

    #endregion

    #region Getters

    private PlayerInfo PlayerInfo => Player.Instance.PlayerInfo;

    #endregion

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    private void Start()
    {
        // Connect to the player's OnDeath event
        PlayerInfo.OnDeath += LoadCheckpoint;
    }

    private void LoadCheckpoint(object sender, HealthChangedEventArgs e)
    {
        // Enable the Death Screen
        if (e.DamagerObject == e.Actor)
            RelapseScreen.Instance.Activate();

        else
        {
            // Determine the player's respawn position
            var respawnPosition = initialSpawnPosition;

            if (_highestCheckpoint > -1)
                respawnPosition = checkpointList[_highestCheckpoint].RespawnPosition;

            // Respawn the player at the respawn position
            RespawnAt(respawnPosition);

            // Reset the player's information when they respawn
            PlayerInfo.ResetPlayer();
        }
    }

    private void RespawnAt(Transform position)
    {
        // Set the position of the player to the position of the checkpoint
        PlayerInfo.transform.position = position.position;
    }

    // When player interacts with a burner phone, save the current checkpoint as the transform of the burner phone
    public void SaveCheckpoint(CheckpointInteractable interactedObject)
    {
        // Set current checkpoint to that object
        _currentCheckpoint = Array.IndexOf(checkpointList, interactedObject);

        // See if the checkpoint is higher than the highest checkpoint
        if (_currentCheckpoint > _highestCheckpoint)
            _highestCheckpoint = _currentCheckpoint;
    }
}