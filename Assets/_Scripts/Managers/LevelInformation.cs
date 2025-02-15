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

        Debug.Log($"LevelInformation for {_sceneName} has been created.");
    }

    private void OnDestroy()
    {
        Instances.Remove(_sceneName);
    }

    public static bool GetLevelInformation(string sceneName, out LevelInformation levelInformation)
    {
        return Instances.TryGetValue(sceneName, out levelInformation);
    }
}