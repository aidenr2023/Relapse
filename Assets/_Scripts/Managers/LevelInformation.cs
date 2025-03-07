using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LevelInformation : MonoBehaviour
{
    private static readonly Dictionary<string, LevelInformation> Instances = new();

    public static event Action<LevelInformation> OnLevelInformationLoaded;
    private static bool _isStaticallyInitialized;

    #region Serialized Fields

    [SerializeField] private string levelName;
    [SerializeField] private bool skipIntroText;
    [SerializeField] private LevelCheckpointCheckpoint startingCheckpoint;

    #endregion

    #region Private Fields

    private string _sceneName;

    #endregion

    #region Getters

    public LevelCheckpointCheckpoint StartingCheckpoint => startingCheckpoint;

    #endregion

    private static void StaticallyInitialize()
    {
        // Return if the static initialization has already been done
        if (_isStaticallyInitialized)
            return;

        OnLevelInformationLoaded += PlayIntroText;

        _isStaticallyInitialized = true;
    }

    private void Awake()
    {
        // Statically initialize the class
        StaticallyInitialize();
        
        _sceneName = gameObject.scene.name;
        Instances.Add(_sceneName, this);
    }

    private void Start()
    {
        OnLevelInformationLoaded?.Invoke(this);
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

    private static void PlayIntroText(LevelInformation levelInfo)
    {
        if (levelInfo.skipIntroText)
            return;

        if (string.IsNullOrEmpty(levelInfo.levelName))
            levelInfo.levelName = "ADD LEVEL NAME";

        // Play the intro text
        Debug.Log($"Playing intro text for {levelInfo.levelName}");

        // Return if the cutscene listener instance is null
        if (CutsceneListener.Instance == null)
        {
            Debug.LogError("CutsceneListener instance not found!");
            return;
        }
        
        CutsceneListener.Instance.PlayBarsAnimation(true);
        CutsceneListener.Instance.LevelNameText.text = levelInfo.levelName;
    }
}