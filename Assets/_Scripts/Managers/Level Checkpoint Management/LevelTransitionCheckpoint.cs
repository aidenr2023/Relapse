using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class LevelTransitionCheckpoint : LevelCheckpointReset
{
    private const float TRANSITION_TIME = .5f;
    private const float TRANSITION_TIME2 = .5f;
    private const float HOLD_TIME = 1f;
    private const float CA_JITTER = 0.1f;
    
    [SerializeField] private bool useLevelInformation = true;
    [SerializeField] private LevelSectionSceneInfo[] scenesToLoad;

    protected override void CustomOnTriggerEnter(Collider other, Player player)
    {
        Debug.Log($"Entered the level transition checkpoint with player {player.name}");

        // Run the transition to the next scene coroutine
        DebugManagerHelper.Instance.StartCoroutine(TransitionToNextScene(scenesToLoad));
    }

    private IEnumerator TransitionToNextScene(LevelSectionSceneInfo[] scenes)
    {
        // Disable the player's controls
        SetPlayerControls(Player.Instance, false);

        // Create a chromatic aberration token
        var caToken =
            PostProcessingVolumeController.Instance.ScreenVolume.ChromaticAberrationModule.Tokens.AddToken(0, -1, true);
        
        // Get the unscaled start time
        var startTime = Time.unscaledTime;

        // While transitioning, fade the screen
        while (Time.unscaledTime - startTime < TRANSITION_TIME)
        {
            var lerpValue = Mathf.InverseLerp(startTime, startTime + TRANSITION_TIME, Time.unscaledTime);
            // TransitionOverlay.Instance.SetOpacity(lerpValue);
            
            var randomValue = Random.Range(-CA_JITTER, CA_JITTER);
            
            caToken.Value = lerpValue + randomValue;

            yield return null;
        }
        
        caToken.Value = 1;
        
        startTime = Time.unscaledTime;
        
        // While transitioning, fade the screen
        while (Time.unscaledTime - startTime < TRANSITION_TIME2)
        {
            var lerpValue = Mathf.InverseLerp(startTime, startTime + TRANSITION_TIME, Time.unscaledTime);
            TransitionOverlay.Instance.SetOpacity(lerpValue);

            yield return null;
        }

        TransitionOverlay.Instance.SetOpacity(1);

        var loadStartTime = Time.unscaledTime;

        //  // Move the player, reset the player to the checkpoint
        // LevelCheckpointManager.Instance.ResetToCheckpoint(LevelCheckpointManager.Instance.CurrentCheckpoint);

        // // Get the position and rotation of the checkpoint
        // var checkpointPosition = LevelCheckpointManager.Instance.CurrentCheckpoint.transform.position;
        // var checkpointRotation = LevelCheckpointManager.Instance.CurrentCheckpoint.transform.rotation;

        // Kill the player's velocity
        Player.Instance.Rigidbody.velocity = Vector3.zero;

        // Unload the current scene
        var operations = LoadNextScenes(scenes);
        // LoadNextSceneSync(scenes);

        yield return null;

        Debug.Log($"After Yield - {useLevelInformation}");

        // Wait for the hold time
        yield return new WaitUntil(() => Time.unscaledTime - loadStartTime >= HOLD_TIME);

        // Wait until all the operations are done
        yield return new WaitUntil(() => operations.All(operation => operation?.isDone ?? true));

        // Kill the player's velocity again
        Player.Instance.Rigidbody.velocity = Vector3.zero;

        if (useLevelInformation)
        {
            // Get the level information for the active scene
            var hasLevelInformation =
                LevelInformation.GetLevelInformation(SceneManager.GetActiveScene().name, out var levelInfo);

            // If the level information exists, set the player's position and rotation to the starting checkpoint
            if (hasLevelInformation)
            {
                if (levelInfo.StartingCheckpoint != null)
                    LevelCheckpointManager.Instance.ResetToCheckpoint(levelInfo.StartingCheckpoint);
                else
                {
                    Player.Instance.Rigidbody.Move(levelInfo.transform.position, Player.Instance.Rigidbody.rotation);
                    Player.Instance.PlayerLook.ApplyRotation(levelInfo.transform.rotation);
                }
            }

            // If the level information does not exist, set the player's position and rotation to the starting checkpoint
            else
            {
                Debug.LogError($"Resetting to checkpoint Old Checkpoint???");
                // Player.Instance.Rigidbody.Move(checkpointPosition, Player.Instance.Rigidbody.rotation);
                // Player.Instance.PlayerLook.ApplyRotation(checkpointRotation);
            }
        }

        // Enable the player's controls
        SetPlayerControls(Player.Instance, true);

        startTime = Time.unscaledTime;

        // While transitioning, fade the screen to black
        while (Time.unscaledTime - startTime < TRANSITION_TIME)
        {
            var lerpValue = 1 - (Time.unscaledTime - startTime) / TRANSITION_TIME;
            TransitionOverlay.Instance.SetOpacity(lerpValue);
            
            yield return null;
        }
        
        // Set the opacity of the transition overlay to 0
        TransitionOverlay.Instance.SetOpacity(0);
        
        startTime = Time.unscaledTime;
        
        // While transitioning, fade the screen to black
        while (Time.unscaledTime - startTime < TRANSITION_TIME)
        {
            var lerpValue = 1 - (Time.unscaledTime - startTime) / TRANSITION_TIME;

            var randomValue = Random.Range(-CA_JITTER, CA_JITTER);
            
            caToken.Value = lerpValue + randomValue;
            
            yield return null;
        }
        
        // Remove the chromatic aberration token
        PostProcessingVolumeController.Instance.ScreenVolume.ChromaticAberrationModule.Tokens.RemoveToken(caToken);
    }

    private static List<AsyncOperation> LoadNextScenes(LevelSectionSceneInfo[] levelInfo)
    {
        // If the array is empty or null, return
        if (levelInfo == null || levelInfo.Length == 0)
            return new List<AsyncOperation>();

        // If there is no player, return
        if (Player.Instance == null)
            return new List<AsyncOperation>();

        // Move the player back to their original scene
        Player.Instance.transform.parent = Player.Instance.OriginalSceneObject.transform;

        // Find the scene the player is in
        var playerSceneField = (SceneField)Player.Instance.gameObject.scene.name;

        // Get the currently managed scenes from the AsyncSceneManager
        var managedScenes = AsyncSceneManager.Instance.GetManagedScenes();

        // Create a level section scene info array with all the managed scenes EXCEPT the player's scene
        var scenesToUnload = new List<string>();
        foreach (var scene in managedScenes)
        {
            if (scene == playerSceneField.SceneName)
                continue;

            scenesToUnload.Add(scene);
        }

        // Convert the scenes to unload to a LevelSectionSceneInfo array
        var scenesToUnloadInfo = scenesToUnload.Select(scene => LevelSectionSceneInfo.Create(null, scene)).ToArray();

        // Create a new SceneLoaderInformation based on the input
        var sceneLoaderInformation = SceneLoaderInformation.Create(levelInfo, scenesToUnloadInfo);

        // Load the scene
        // AsyncSceneManager.Instance.DebugLoadSceneSynchronous(sceneLoaderInformation);
        var operations = AsyncSceneManager.Instance.LoadSceneAsync(sceneLoaderInformation, true);

        // Set the parent of the player back to null
        Player.Instance.transform.parent = null;

        return operations;
    }

    private static void SetPlayerControls(Player player, bool isOn)
    {
        var playerMovementV2 = player.PlayerController as PlayerMovementV2;

        // Return if the player movement is null
        if (playerMovementV2 == null)
            return;

        if (isOn)
            playerMovementV2.EnablePlayerControls();
        else
            playerMovementV2.DisablePlayerControls();

        player.WeaponManager.enabled = isOn;
    }
}