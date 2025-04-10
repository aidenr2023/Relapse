using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class HordeModeManager : MonoBehaviour
{
    public static Option<HordeModeManager> Instance { get; private set; } = Option<HordeModeManager>.None;

    [SerializeField] private PlayerInfoEventReference onPlayerRespawnSo;
    [SerializeField] private UnityEvent<PlayerInfo> onPlayerRespawned;

    [SerializeField] private SceneField[] hordeCombatScenes;

    [SerializeField] private Transform spawnPosition;
    [SerializeField] private HordeModeVendorInteractable vendor;
    [SerializeField, Min(0)] private float moveBackToVendorDelay = 3f;

    private Result<Scene> _currentCombatScene = Result<Scene>.Error("Current combat scene is null");
    private bool _isLoading = false;

    private void OnEnable()
    {
        // Set the instance
        Instance = this.ToSome();

        // Subscribe to the player respawn event
        onPlayerRespawnSo += UnloadCurrentCombatSceneOnRespawn;
        onPlayerRespawnSo += onPlayerRespawned.Invoke;
    }

    private void UnloadCurrentCombatSceneOnRespawn(PlayerInfo playerInfo)
    {
        // Unload the current combat scene
        UnloadCurrentCombatScene();
    }

    private void OnDisable()
    {
        // If the instance is not none, set it to none
        Instance.Match(_ => Instance = Option<HordeModeManager>.None);

        // Unubscribe to the player respawn event
        onPlayerRespawnSo -= UnloadCurrentCombatSceneOnRespawn;
    }

    /// <summary>
    /// Load one of the combat scenes either synchronously or asynchronously.
    /// </summary>
    /// <param name="index"></param>
    /// <param name="async"></param>
    private Result<bool> LoadCombatScene(int index, bool async)
    {
        // Return if the index is out of bounds
        if (index < 0 || index >= hordeCombatScenes.Length)
            return Result<bool>.Error("Index is out of bounds");

        // If the scene is currently loading, return
        if (_isLoading)
            return Result<bool>.Error("Scene is currently loading");

        // Force Unload the current combat scene
        UnloadCurrentCombatScene();

        var sceneToLoad = hordeCombatScenes[index];

        // Load the scene
        if (async)
        {
            // Load the scene asynchronously
            var asyncOperation = SceneManager.LoadSceneAsync(sceneToLoad.SceneName, LoadSceneMode.Additive);

            if (asyncOperation == null)
                return Result<bool>.Error($"Failed to load scene {sceneToLoad.SceneName}");

            // Set the async operation to activate the scene
            asyncOperation.allowSceneActivation = true;

            // Set the isLoading variable to true
            _isLoading = true;

            // Set the current combat scene to the loaded scene
            asyncOperation.completed += _ =>
            {
                var loadedScene = SceneManager.GetSceneByName(sceneToLoad.SceneName);
                _currentCombatScene = Result<Scene>.Ok(loadedScene);

                // Set the isLoading variable to false
                _isLoading = false;
            };
        }
        else
        {
            // Load the scene synchronously
            SceneManager.LoadScene(sceneToLoad.SceneName, LoadSceneMode.Additive);
            _currentCombatScene = Result<Scene>.Ok(SceneManager.GetSceneByName(sceneToLoad.SceneName));
        }

        // Return a success result
        return Result<bool>.Ok(true);
    }

    private void UnloadCurrentCombatScene()
    {
        // If the scene is loaded, unload it    
        _currentCombatScene
            .Match(scene => SceneManager.UnloadSceneAsync(scene))
            .Match(_ => _currentCombatScene = Result<Scene>.Error("Current combat scene is null"));
    }

    [ContextMenu("Load Random Combat Scene")]
    public void LoadRandomCombatScene()
    {
        // Get a random index
        var randomIndex = Random.Range(0, hordeCombatScenes.Length);

        // Load the scene at the random index
        LoadCombatScene(randomIndex, true)
            .ReadError(n => Debug.LogError($"Failed to load random combat scene: {n}"));
    }

    public void MovePlayerBackToVendor()
    {
        // Get the instance of the player
        var playerInstance = Player.Instance;

        // Check if the player instance is null
        if (playerInstance == null)
        {
            Debug.LogError("Player instance is null. Cannot move player to vendor.");
            return;
        }

        // Start the coroutine to move the player back to the vendor
        StartCoroutine(MovePlayerBackToVendorCoroutine(playerInstance, spawnPosition, moveBackToVendorDelay));
    }

    private IEnumerator MovePlayerBackToVendorCoroutine(Player playerInstance, Transform vendorPosition, float delay)
    {
        var startTime = Time.time;

        // Create a new tooltip saying how long until the player is moved
        JournalTooltipManager.Instance.AddTooltip(
            () => $"Returning to the vendor in {delay - (int)(Time.time - startTime + 1)} seconds",
            delay, true, () => Time.time - startTime >= delay
        );

        yield return new WaitForSeconds(delay);

        // Move the player to the vendor's position
        playerInstance.Rigidbody.MovePosition(vendorPosition.position);

        // Make the player face the direction of the vendor
        playerInstance.PlayerLook.ApplyRotation(vendorPosition.rotation);

        // Forcibly unload the current combat scene
        UnloadCurrentCombatScene();
    }

    public void ReinitializeVendor()
    {
        // Reinitialize the vendor
        vendor.Reinitialize();
    }
}