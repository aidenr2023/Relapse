using UnityEngine;

public class LevelCheckpointManager
{
    #region Singleton Pattern

    private static LevelCheckpointManager _instance;

    public static LevelCheckpointManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new LevelCheckpointManager();

            return _instance;
        }
    }

    #endregion

    public LevelCheckpointCheckpoint CurrentCheckpoint { get; private set; }

    public void SetCheckpoint(LevelCheckpointCheckpoint checkpoint)
    {
        // Return if the checkpoint is null
        if (checkpoint == null)
            return;

        // If the current checkpoint equals the checkpoint, return
        if (CurrentCheckpoint == checkpoint)
            return;

        // If the checkpoint's respawn point is null, Log an error and return
        if (checkpoint.RespawnPoint == null)
        {
            Debug.LogError("Checkpoint's Respawn Point is null!!!", checkpoint);
            return;
        }

        // Set the current checkpoint
        CurrentCheckpoint = checkpoint;
        
        // // Log the checkpoint
        // Debug.Log($"Checkpoint set to {checkpoint.name}");
    }

    public void ResetToCheckpoint(LevelCheckpointCheckpoint checkpoint)
    {
        // Return if the checkpoint is null
        if (checkpoint == null)
            return;

        // If the checkpoint's respawn point is null, Log an error and return
        if (checkpoint.RespawnPoint == null)
        {
            Debug.LogError("Checkpoint's Respawn Point is null!!!", checkpoint);
            return;
        }

        // // Move the player to the checkpoint's respawn point
        // // Rotate the player to the checkpoint's rotation
        // Player.Instance.transform.position = checkpoint.RespawnPoint.position;
        
        // Convert the respawn forward to a quaternion
        var respawnForward = Quaternion.LookRotation(checkpoint.RespawnForward);
        Player.Instance.PlayerLook.ApplyRotation(respawnForward);
        
        Player.Instance.Rigidbody.MovePosition(checkpoint.RespawnPoint.position);
        
        // Kill the player's velocity
        Player.Instance.Rigidbody.velocity = Vector3.zero;
        
        Debug.Log($"Player reset to {checkpoint.name}");
    }
}