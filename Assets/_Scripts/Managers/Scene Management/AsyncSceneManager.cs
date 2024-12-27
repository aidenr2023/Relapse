using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AsyncSceneManager
{
    private static AsyncSceneManager _instance;

    public static AsyncSceneManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new AsyncSceneManager();

            return _instance;
        }
    }

    #region Private Fields

    private readonly Dictionary<string, AsyncSceneRecord> _asyncSceneRecords = new();

    private readonly Dictionary<string, GameObject[]> _disabledSceneObjects = new();

    #endregion

    private AsyncSceneManager()
    {
        // Set the instance
        if (_instance == null)
            _instance = this;
    }

    #region Scene Methods

    private AsyncOperation LoadSceneAsync(SceneField scene)
    {
        // If there is already a record for the scene
        if (_asyncSceneRecords.TryGetValue(scene, out var sceneRecord))
        {
            switch (sceneRecord.State)
            {
                // If the scene is currently loading, return
                case AsyncSceneState.Loading:
                    return sceneRecord.Operation;

                // If the scene is already loaded, return
                case AsyncSceneState.Loaded:
                    return sceneRecord.Operation;

                // If the scene is canceled, set the async operation to finish loading
                case AsyncSceneState.Canceled:
                    sceneRecord.State = AsyncSceneState.Loading;

                    if (sceneRecord.Operation != null)
                        sceneRecord.Operation.allowSceneActivation = true;
                    return sceneRecord.Operation;

                // If the scene is unloaded, remove it from the dictionary and call the function again
                case AsyncSceneState.Unloaded:
                    _asyncSceneRecords.Remove(scene);
                    LoadSceneAsync(scene);
                    return sceneRecord.Operation;

                // If the scene is disabled, enable it
                case AsyncSceneState.Disabled:

                    // Enable the scene
                    EnableScene(sceneRecord.Scene);

                    // Set the scene state to loaded
                    sceneRecord.State = AsyncSceneState.Loaded;

                    return sceneRecord.Operation;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        // TODO: Delete this
        // Add a tooltip to the scene field
        JournalTooltipManager.Instance?.AddTooltip($"Loading {scene.SceneName}...");

        // If there is NOT a record already for the scene, make one
        var operation = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);

        // If the operation is null, throw an exception
        if (operation == null)
            throw new Exception("Operation is null");

        operation.completed += _ =>
        {
            _asyncSceneRecords[scene.SceneName].State = AsyncSceneState.Loaded;
            _asyncSceneRecords[scene.SceneName].Scene = SceneManager.GetSceneByName(scene);
            JournalTooltipManager.Instance.AddTooltip($"Done Loading {scene.SceneName}!");

            // Debug.Log($"After loading scene {scene.SceneName} - {_asyncSceneRecords[scene.SceneName].Scene.name}");
        };

        // Create the record
        sceneRecord = new AsyncSceneRecord(scene, operation, AsyncSceneState.Loading);

        // Add the record to the dictionary
        _asyncSceneRecords.Add(scene.SceneName, sceneRecord);

        Debug.Log($"Currently managed scenes: {string.Join(", ", _asyncSceneRecords.Keys)}");

        return operation;
    }

    public void LoadSceneAsync(SceneLoaderInformation sceneLoader)
    {
        // Unload all the scenes to unload
        var scenesToUnload = sceneLoader.SectionsToUnload;

        foreach (var scene in scenesToUnload)
            UnloadSceneAsync(scene);

        // Load all the scenes to load
        var scenesToLoad = sceneLoader.SectionsToLoad;

        foreach (var scene in scenesToLoad)
            LoadSceneAsync(scene);
    }

    public void LoadSceneAsync(LevelSectionSceneInfo levelSectionSceneInfo)
    {
        // Load the section scene
        if (levelSectionSceneInfo.SectionScene != null &&
            levelSectionSceneInfo.SectionScene.SceneName != ""
           )
            LoadSceneAsync(levelSectionSceneInfo.SectionScene);

        // Load the section persistent data
        if (levelSectionSceneInfo.SectionPersistentData != null &&
            levelSectionSceneInfo.SectionPersistentData.SceneName != ""
           )
            LoadSceneAsync(levelSectionSceneInfo.SectionPersistentData);
    }

    private void LoadSceneSynchronous(SceneField scene)
    {
        // If the scene is already loaded, return
        if (_asyncSceneRecords.TryGetValue(scene, out var sceneRecord))
            return;

        // Load the scene
        SceneManager.LoadScene(scene, LoadSceneMode.Additive);

        // Get the scene that was just loaded
        var sceneLoaded = SceneManager.GetSceneByName(scene);

        // Create a record for the scene
        var record = new AsyncSceneRecord(scene, null, AsyncSceneState.Loaded);
        record.Scene = sceneLoaded;

        // Add the record to the dictionary
        _asyncSceneRecords.Add(scene.SceneName, record);
    }

    public void LoadSceneSynchronous(LevelSectionSceneInfo levelSectionSceneInfo)
    {
        // Load the section scene
        if (levelSectionSceneInfo.SectionScene != null)
            LoadSceneSynchronous(levelSectionSceneInfo.SectionScene);

        // Load the section persistent data
        if (levelSectionSceneInfo.SectionPersistentData != null)
            LoadSceneSynchronous(levelSectionSceneInfo.SectionPersistentData);
    }

    private void UnloadSceneAsync(SceneUnloadField scene)
    {
        // If there is no record, the scene is not managed by the scene manager / not open
        if (!_asyncSceneRecords.TryGetValue(scene, out var sceneRecord))
            return;

        Debug.Log($"Unloading scene {scene.SceneField.SceneName} - {sceneRecord.State}");

        switch (sceneRecord.State)
        {
            // If the scene is unloaded, remove it from the dictionary
            case AsyncSceneState.Unloaded:
                _asyncSceneRecords.Remove(scene);
                return;

            // If the scene is loading, cancel the operation
            case AsyncSceneState.Loading:

                if (!scene.IsDisableInstead)
                {
                    sceneRecord.State = AsyncSceneState.Canceled;

                    if (sceneRecord.Operation != null)
                        sceneRecord.Operation.allowSceneActivation = false;
                }

                else
                {
                    // Set the operation to disable the scene once loading is complete
                    sceneRecord.Operation.completed += _ =>
                    {
                        sceneRecord.State = AsyncSceneState.Disabled;

                        // Get the actual scene reference
                        var sceneToDisable = sceneRecord.Scene;

                        // Disable the scene
                        DisableScene(sceneToDisable);
                    };
                }

                return;

            // If the scene is loaded, unload it
            case AsyncSceneState.Loaded:
                if (!scene.IsDisableInstead)
                {
                    SceneManager.UnloadSceneAsync(sceneRecord.SceneField);
                    _asyncSceneRecords.Remove(scene);
                }

                else
                {
                    sceneRecord.State = AsyncSceneState.Disabled;

                    // Get the actual scene reference
                    var sceneToDisable = sceneRecord.Scene;

                    // Disable the scene
                    DisableScene(sceneToDisable);
                    return;
                }

                return;

            // If the scene is canceled, return
            case AsyncSceneState.Canceled:
                return;

            // If the scene is disabled, return
            case AsyncSceneState.Disabled:
                return;
        }
    }

    public void UnloadSceneAsync(LevelSectionSceneInfo levelSectionSceneInfo)
    {
        // Unload the section scene
        if (levelSectionSceneInfo.SectionScene != null)
        {
            // Create a scene unload field
            var sceneUnloadField = SceneUnloadField.Create(levelSectionSceneInfo.SectionScene, false);

            // Unload the scene
            UnloadSceneAsync(sceneUnloadField);
        }

        // Unload the section persistent data
        if (levelSectionSceneInfo.SectionPersistentData != null)
        {
            Debug.Log(
                $"Unloading Persistent Data: {levelSectionSceneInfo.SectionPersistentData.SceneName} - {levelSectionSceneInfo}");

            // Create a scene unload field that disables the scene instead of unloading it
            var sceneUnloadField = SceneUnloadField.Create(levelSectionSceneInfo.SectionPersistentData, true);

            // Unload the scene
            UnloadSceneAsync(sceneUnloadField);
        }
    }

    private void DisableScene(Scene scene)
    {
        // Get all the root game objects in the scene
        var rootGameObjects = scene.GetRootGameObjects();

        var objectsToTrack = rootGameObjects
            .Where(n => n.activeSelf)
            .ToArray();

        // Add the objects to the dictionary
        _disabledSceneObjects.Add(scene.name, objectsToTrack);

        // Disable all the objects
        foreach (var obj in objectsToTrack)
            obj.SetActive(false);
    }

    private void EnableScene(Scene scene)
    {
        // Return if the scene is not in the dictionary
        if (!_disabledSceneObjects.TryGetValue(scene.name, out var objectsToEnable))
            return;

        // Enable all the objects
        foreach (var obj in objectsToEnable)
            obj.SetActive(true);

        // Remove the objects from the dictionary
        _disabledSceneObjects.Remove(scene.name);
    }

    public void ForceManageScene(SceneField sceneField)
    {
        // Return if the scene is not currently open
        if (SceneManager.GetSceneByName(sceneField).name != sceneField.SceneName)
            return;

        // Return if a key already exists for the scene
        if (_asyncSceneRecords.ContainsKey(sceneField.SceneName))
            return;

        // Create a new record for the scene
        var sceneRecord = new AsyncSceneRecord(sceneField, null, AsyncSceneState.Loaded);

        // Get the scene reference
        sceneRecord.Scene = SceneManager.GetSceneByName(sceneField);

        // Add the record to the dictionary
        _asyncSceneRecords.Add(sceneField.SceneName, sceneRecord);

        // Debug.Log($"Forced management of scene {sceneField.SceneName}");
    }

    public void ForceManageScene(LevelSectionSceneInfo sceneLoaderInformation)
    {
        // Load the section scene
        if (sceneLoaderInformation.SectionScene != null)
            ForceManageScene(sceneLoaderInformation.SectionScene);

        // Load the section persistent data
        if (sceneLoaderInformation.SectionPersistentData != null)
            ForceManageScene(sceneLoaderInformation.SectionPersistentData);
    }

    public void LoadStartupScene(LevelStartupSceneInfo startupSceneInfo, MonoBehaviour coroutineRunner,
        Action<float> percentageCallback = null)
    {
        // Return if the coroutine runner is null
        if (coroutineRunner == null)
        {
            Debug.LogError("Coroutine runner is null!");
            return;
        }

        // Return if the startup scene info is null
        if (startupSceneInfo == null)
        {
            Debug.LogError("Startup scene info is null!");
            return;
        }

        // Start the coroutine
        coroutineRunner.StartCoroutine(LoadMultipleScenes(startupSceneInfo, coroutineRunner, percentageCallback));
    }

    private IEnumerator LoadMultipleScenes(LevelStartupSceneInfo startupSceneInfo, MonoBehaviour coroutineRunner,
        Action<float> percentageCallback)
    {
        List<Scene> scenesToUnload = new();

        // Get all currently loaded scenes
        for (var i = 0; i < SceneManager.sceneCount; i++)
            scenesToUnload.Add(SceneManager.GetSceneAt(i));

        // Clear the currently managed scenes
        _asyncSceneRecords.Clear();

        // Clear the currently disabled scene objects
        _disabledSceneObjects.Clear();

        // Create a hash set to store all the load operations
        HashSet<AsyncOperation> loadOperations = new();

        AsyncOperation activeSceneOp = null;

        // Load all the startup sections
        foreach (var section in startupSceneInfo.StartupSections)
        {
            // Load the section scene
            if (section.SectionScene != null)
            {
                var operation = LoadSceneAsync(section.SectionScene);

                // Set the scene to not be activated
                operation.allowSceneActivation = false;

                // Add the operation to the hash set
                loadOperations.Add(operation);

                // If this scene is the active scene, set the operation to set the active scene
                if (section.SectionScene.SceneName == startupSceneInfo.ActiveScene.SceneName)
                    activeSceneOp = operation;
            }

            // Load the section persistent data
            if (section.SectionPersistentData != null)
            {
                var operation = LoadSceneAsync(section.SectionPersistentData);

                // Set the scene to not be activated
                operation.allowSceneActivation = false;

                // Add the operation to the hash set
                loadOperations.Add(operation);

                // If this scene is the active scene, set the operation to set the active scene
                if (section.SectionPersistentData.SceneName == startupSceneInfo.ActiveScene.SceneName)
                    activeSceneOp = operation;
            }
        }

        // Wait while the operations are not done
        while (loadOperations.Any(n => n.progress < 0.9f))
        {
            // Calculate the total progress
            var loadProgress = loadOperations.Sum(n => n.progress) / loadOperations.Count;

            percentageCallback?.Invoke(loadProgress);
            yield return null;
        }

        // Load the player data scene
        if (startupSceneInfo.PlayerDataScene != null)
        {
            var operation = LoadSceneAsync(startupSceneInfo.PlayerDataScene);

            // Set the scene to not be activated
            operation.allowSceneActivation = false;

            // Add the operation to the hash set
            loadOperations.Add(operation);

            // If this scene is the active scene, set the operation to set the active scene
            if (startupSceneInfo.PlayerDataScene.SceneName == startupSceneInfo.ActiveScene.SceneName)
                activeSceneOp = operation;
        }

        // Set all the load operations to allow scene activation
        foreach (var operation in loadOperations)
            operation.allowSceneActivation = true;

        var emptyScene = SceneManager.CreateScene("Empty");

        // Unload all the currently loaded scenes asynchronously,
        // but don't remove them from the hierarchy yet
        foreach (var scene in scenesToUnload)
        {
            var operation = SceneManager.UnloadSceneAsync(scene.name);

            // Set the scene to be activated
            operation.allowSceneActivation = true;
        }

        // if the active scene is not done, wait
        if (activeSceneOp != null)
        {
            yield return new WaitUntil(() => activeSceneOp.isDone);

            // Get the scene reference for the active scene
            var activeScene = SceneManager.GetSceneByName(startupSceneInfo.ActiveScene);

            // Set the active scene to the active scene
            SceneManager.SetActiveScene(activeScene);
        }

        // Unload the empty scene
        var emptySceneOp = SceneManager.UnloadSceneAsync(emptyScene);
        emptySceneOp.allowSceneActivation = true;

        Debug.Log($"All Operations Done! - {startupSceneInfo.ActiveScene.SceneName}");
    }

    #endregion

    private enum AsyncSceneState
    {
        Unloaded,
        Loading,
        Canceled,
        Disabled,
        Loaded,
    }

    private record AsyncSceneRecord(SceneField SceneField, AsyncOperation Operation, AsyncSceneState State)
    {
        public SceneField SceneField { get; private set; } = SceneField;
        public AsyncOperation Operation { get; private set; } = Operation;
        public AsyncSceneState State { get; set; } = State;
        public Scene Scene { get; set; } = default;
    }
}