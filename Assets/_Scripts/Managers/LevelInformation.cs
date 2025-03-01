using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LevelInformation : MonoBehaviour
{
    private static readonly Dictionary<string, LevelInformation> Instances = new();

    #region Serialized Fields

    [SerializeField] private LevelCheckpointCheckpoint startingCheckpoint;

    #endregion

    #region Private Fields

    private string _sceneName;

    #endregion

    #region Getters

    public LevelCheckpointCheckpoint StartingCheckpoint => startingCheckpoint;

    #endregion

    private void Awake()
    {
        _sceneName = gameObject.scene.name;
        Instances.Add(_sceneName, this);
    }

    private void OnDestroy()
    {
        Instances.Remove(_sceneName);
    }

    public static bool GetLevelInformation(string sceneName, out LevelInformation levelInformation)
    {
        return Instances.TryGetValue(sceneName, out levelInformation);
    }

    private void OnDrawGizmos()
    {
        // Return if the starting checkpoint is not null
        if (startingCheckpoint != null)
            return;

        // Draw a line from the starting checkpoint to the player
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 0.5f);
        CustomFunctions.DrawArrow(transform.position + Vector3.up * 1, transform.forward, 0.5f);
    }
}