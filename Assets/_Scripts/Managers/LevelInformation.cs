using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelInformation : MonoBehaviour
{
    private static readonly Dictionary<string, LevelInformation> Instances = new();

    public static event Action<LevelInformation> OnLevelInformationLoaded;
    private static bool _isStaticallyInitialized;

    #region Serialized Fields

    [SerializeField] private string levelName;
    [SerializeField] private string levelSubtitle;
    [SerializeField] private bool skipIntroText;
    [SerializeField] private LevelCheckpointCheckpoint startingCheckpoint;

    [Header("Scene Manager Stuff"), SerializeField]
    private bool addToCheckpointManagerOnLoad;

    [SerializeField] private LevelSectionSceneInfo forcedSectionSceneInfo;

    [Header("Difficulty"), SerializeField] private FloatReference difficultyHealthMultiplierSo;
    [SerializeField] private FloatReference difficultyDamageMultiplierSo;
    [SerializeField] private DifficultySettings difficultySettings;

    #endregion

    #region Private Fields

    private string _sceneName;

    #endregion

    #region Getters

    public LevelCheckpointCheckpoint StartingCheckpoint => startingCheckpoint;

    private Vector3 StartingPosition
    {
        get
        {
            // Return the respawn point of the starting checkpoint
            if (startingCheckpoint != null && startingCheckpoint.RespawnPoint != null)
                return startingCheckpoint.RespawnPoint.position;

            // Otherwise, return the transform position
            return transform.position;
        }
    }

    #endregion

    private static void StaticallyInitialize()
    {
        // Return if the static initialization has already been done
        if (_isStaticallyInitialized)
            return;

        OnLevelInformationLoaded += ForceSceneInformation;
        OnLevelInformationLoaded += PlayIntroText;
        OnLevelInformationLoaded += AddToCheckpointManager;

        _isStaticallyInitialized = true;
    }

    private static void ForceSceneInformation(LevelInformation obj)
    {
        // Return if the forced section scene info is null
        if (obj.forcedSectionSceneInfo == null)
            return;

        AsyncSceneManager.Instance.ForceManageScene(obj.forcedSectionSceneInfo);
    }

    private static void AddToCheckpointManager(LevelInformation obj)
    {
        // Return if the game is not running
        if (!Application.isPlaying)
            return;

        if (!obj.addToCheckpointManagerOnLoad)
            return;
        
        // Debug.Log($"Adding {obj.gameObject.name} to the checkpoint manager -> {obj.StartingPosition}", obj);

        obj.StartCoroutine(TryToAddToCheckpointManager(obj));
    }

    private static IEnumerator TryToAddToCheckpointManager(LevelInformation obj)
    {
        // Wait for the checkpoint manager to be initialized
        yield return new WaitUntil(() => CheckpointManager.Instance != null);

        // yield return new WaitForSeconds(1f);
        yield return null;

        var checkpointManager = CheckpointManager.Instance;

        // Save the starting checkpoint (if necessary)
        checkpointManager.SaveCheckpoint(obj.StartingPosition);

        // // Save the checkpoint information
        // CheckpointInteractable.SaveInformation();

        // // Save the scene information
        // SceneSaveLoader.Instance.SaveSettingsToDisk();
    }

    private void Awake()
    {
        // Statically initialize the class
        StaticallyInitialize();

        // Apply the difficulty multiplier
        ApplyDifficultyMultiplier();

        _sceneName = gameObject.scene.name;
        Instances.Add(_sceneName, this);
    }

    private void ApplyDifficultyMultiplier()
    {
        // Return if not in play mode
        if (!Application.isPlaying)
            return;

        // Return if the difficulty settings are null
        if (difficultySettings == null)
            return;

        // Set the difficulty multipliers
        if (difficultyHealthMultiplierSo != null)
        {
            try
            {
                difficultyHealthMultiplierSo.Value = difficultySettings.DifficultyHealthMultiplier;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        if (difficultyDamageMultiplierSo != null)
        {
            try
            {
                difficultyDamageMultiplierSo.Value = difficultySettings.DifficultyDamageMultiplier;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
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

        // Return if the cutscene listener instance is null
        if (CutsceneListener.Instance == null && Application.isPlaying)
        {
            Debug.LogError("CutsceneListener instance not found!");
            return;
        }

        CutsceneListener.Instance.PlayBarsAnimation(true);
        CutsceneListener.Instance.LevelNameText.text = levelInfo.levelName;
        CutsceneListener.Instance.LevelSubtitleText.text = levelInfo.levelSubtitle;
    }
}