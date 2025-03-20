using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AsyncSceneManager : IDebugged
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

    private readonly HashSet<string> _scenesThatNeedToLoadFromDisk = new();

    private readonly HashSet<string> _debugLoadedScenes = new();

    #endregion

    #region Getters

    public IReadOnlyList<string> GetManagedScenes() =>
        _asyncSceneRecords
            .Where(n => n.Value.State == AsyncSceneState.Loaded)
            .Select(n => n.Key)
            .ToArray();

    public LevelSectionSceneInfo CurrentSceneInfo { get; private set; }

    #endregion

    private event Action<LevelSectionSceneInfo> OnSectionLoadCompletion;

    private AsyncSceneManager()
    {
        // Set the instance
        if (_instance == null)
            _instance = this;

        // Add the scene manager to the debug manager
        DebugManager.Instance.AddDebuggedObject(this);

        SceneManager.sceneLoaded += LoadDataFromDiskOnSceneLoaded;
        SceneManager.sceneLoaded += SceneLoadedDebug;

        // on section scene load events
        OnSectionLoadCompletion += SetCurrentSceneInfo;
        OnSectionLoadCompletion += SetMovementTypeOnSectionLoad;
        OnSectionLoadCompletion += SetPostProcessingOnSectionLoad;
    }


    #region Events

    private void SetMovementTypeOnSectionLoad(LevelSectionSceneInfo levelSectionSceneInfo)
    {
        // Set the player movement type
        SetPlayerMovementType(levelSectionSceneInfo.PlayerMovementType);
    }

    private void SetPostProcessingOnSectionLoad(LevelSectionSceneInfo levelSectionSceneInfo)
    {
        PostProcessingVolumeController.Instance.ChangePostProcessing(levelSectionSceneInfo.PostProcessingType, 0);
    }

    private void SetCurrentSceneInfo(LevelSectionSceneInfo obj)
    {
        CurrentSceneInfo = obj;
    }

    #endregion

    private void SceneLoadedDebug(Scene scene, LoadSceneMode _)
    {
        // If this scene is not a debug loaded scene, return
        if (!_debugLoadedScenes.Remove(scene.name))
            return;

        Debug.Log($"Trying to load scene: {scene.name}");

        // Try to get the level information for the loaded scene
        if (!LevelInformation.GetLevelInformation(scene.name, out var levelInfo))
        {
            // // Log an error if the level information is not found
            // Debug.LogError($"Level Information for {scene.name} not found!");
            return;
        }

        Debug.Log($"Loaded Level: {levelInfo.name}");

        if (levelInfo.StartingCheckpoint != null)
        {
            Debug.Log($"Starting Checkpoint: {levelInfo.StartingCheckpoint.name}");
            LevelCheckpointManager.Instance.ResetToCheckpoint(levelInfo.StartingCheckpoint);
        }
        else
        {
            // Kill the player's velocity
            Player.Instance.Rigidbody.velocity = Vector3.zero;

            // Move the player to the level information's position
            Player.Instance.Rigidbody.position = levelInfo.transform.position;

            // Rotate the player to the level information's rotation
            Player.Instance.PlayerLook.ApplyRotation(levelInfo.transform.rotation);

            Debug.Log($"Moving player to {levelInfo.name} ({levelInfo.transform.position})");
        }
    }

    private void LoadDataFromDiskOnSceneLoaded(Scene scene, LoadSceneMode _)
    {
        // If the scene is in the list of scenes that need to load from disk, load the data
        if (!_scenesThatNeedToLoadFromDisk.Contains(scene.name))
            return;

        // Debug.Log($"Running the event callback for {scene.name}");

        // Remove the scene from the list
        _scenesThatNeedToLoadFromDisk.Remove(scene.name);

        // Load the data from the disk to memory
        LevelLoader.Instance.LoadDataDiskToMemory(scene);

        // Load the data from memory to the scene
        LevelLoader.Instance.LoadDataMemoryToScene(scene);
    }

    #region Scene Methods

    private AsyncOperation LoadSceneAsync(SceneField scene, bool loadInfoFromMemory = false)
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
                    LoadSceneAsync(scene, loadInfoFromMemory);
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
        if (DebugManager.Instance.IsDebugMode)
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
            if (DebugManager.Instance.IsDebugMode)
                JournalTooltipManager.Instance.AddTooltip($"Done Loading {scene.SceneName}!");

            // Debug.Log($"After loading scene {scene.SceneName} - {_asyncSceneRecords[scene.SceneName].Scene.name}");
        };

        // Create the record
        sceneRecord = new AsyncSceneRecord(scene, operation, AsyncSceneState.Loading);

        // Add the record to the dictionary
        _asyncSceneRecords.Add(scene.SceneName, sceneRecord);

        // Debug.Log($"Currently managed scenes: {string.Join(", ", _asyncSceneRecords.Keys)}");

        if (loadInfoFromMemory)
        {
            operation.completed += _ =>
            {
                var levelLoaderInstance = LevelLoader.Instance;

                if (levelLoaderInstance == null)
                    return;

                levelLoaderInstance.LoadDataMemoryToScene(SceneManager.GetSceneByName(scene));
            };
        }

        return operation;
    }

    public List<AsyncOperation> LoadSceneAsync(SceneLoaderInformation sceneLoader, bool loadInfoFromMemory = false)
    {
        var scenesToUnload = sceneLoader.SectionsToUnload;
        var scenesToLoad = sceneLoader.SectionsToLoad;

        var levelLoaderInstance = LevelLoader.Instance;

        if (loadInfoFromMemory && levelLoaderInstance != null)
        {
            // First, save the data from the unloading scenes to the memory
            foreach (var sceneInfo in scenesToUnload)
            {
                // Continue if the scene info is null
                if (sceneInfo == null)
                    continue;

                var persistentDataScene = SceneManager.GetSceneByName(sceneInfo.SectionPersistentData?.SceneName ?? "");

                // Save the persistent data scene to memory
                if (sceneInfo.SectionPersistentData != null &&
                    !string.IsNullOrEmpty(sceneInfo.SectionPersistentData.SceneName) &&
                    persistentDataScene.isLoaded
                   )
                {
                    // Debug.Log($"Saving data for {sceneInfo.SectionPersistentData.SceneName} to memory.");
                    levelLoaderInstance.SaveDataSceneToMemory(persistentDataScene);
                }

                var sectionScene = SceneManager.GetSceneByName(sceneInfo.SectionScene);

                // Save the section scene to memory
                if (sceneInfo.SectionScene != null && sceneInfo.SectionScene.SceneName != "" && sectionScene.isLoaded)
                {
                    // Debug.Log($"Saving data for {sceneInfo.SectionScene.SceneName} to memory.");
                    levelLoaderInstance.SaveDataSceneToMemory(sectionScene);
                }
            }
        }

        // Unload all the scenes to unload
        foreach (var scene in scenesToUnload)
            UnloadSceneAsync(scene);

        var operations = new List<AsyncOperation>();

        // Load all the scenes to load
        foreach (var scene in scenesToLoad)
            operations.AddRange(LoadSceneAsync(scene, loadInfoFromMemory));

        return operations;
    }

    public List<AsyncOperation> LoadSceneAsync(LevelSectionSceneInfo levelSectionSceneInfo,
        bool loadInfoFromMemory = false)
    {
        AsyncOperation sectionSceneOp = null;
        AsyncOperation sectionPersistentDataOp = null;

        // Load the section scene
        if (levelSectionSceneInfo.SectionScene != null &&
            levelSectionSceneInfo.SectionScene.SceneName != ""
           )
            sectionSceneOp = LoadSceneAsync(levelSectionSceneInfo.SectionScene, loadInfoFromMemory);

        // Load the section persistent data
        if (levelSectionSceneInfo.SectionPersistentData != null &&
            levelSectionSceneInfo.SectionPersistentData.SceneName != ""
           )
            sectionPersistentDataOp = LoadSceneAsync(levelSectionSceneInfo.SectionPersistentData, loadInfoFromMemory);


        if (sectionSceneOp == null)
            return new List<AsyncOperation>();

        // When the section scene is done loading, set the active scene
        sectionSceneOp.completed += operation =>
        {
            if (!levelSectionSceneInfo.SetActiveSceneToSectionScene)
                return;

            var activeScene = SceneManager.GetSceneByName(levelSectionSceneInfo.SectionScene);

            if (activeScene.IsValid())
                SceneManager.SetActiveScene(activeScene);

            OnSectionLoadCompletion?.Invoke(levelSectionSceneInfo);
        };

        // Return the operations
        return new List<AsyncOperation> { sectionSceneOp, sectionPersistentDataOp };
    }

    private void LoadSceneSynchronous(SceneField scene)
    {
        // If the scene is already loaded, return
        if (_asyncSceneRecords.TryGetValue(scene, out var sceneRecord))
            return;

        // Return if the scene name is empty
        if (scene.SceneName.Trim() == "")
        {
            Debug.LogWarning("Scene name is empty!");
            return;
        }

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

        // // If the scene is an active scene, set the player movement type
        // if (levelSectionSceneInfo.SetActiveSceneToSectionScene)
        //     SetPlayerMovementType(levelSectionSceneInfo.PlayerMovementType);

        // If the scene is an active scene, invoke the event
        if (levelSectionSceneInfo.SetActiveSceneToSectionScene)
            OnSectionLoadCompletion?.Invoke(levelSectionSceneInfo);
    }

    public void LoadScenesSynchronous(SceneLoaderInformation loaderInformation)
    {
        // Unload all the scenes to unload asynchronously
        foreach (var scene in loaderInformation.SectionsToUnload)
            UnloadSceneAsync(scene);

        // Load all the scenes to load synchronously
        foreach (var scene in loaderInformation.SectionsToLoad)
            LoadSceneSynchronous(scene);
    }

    private void UnloadSceneAsync(SceneUnloadField scene, Action onCompletion = null)
    {
        // If there is no record, the scene is not managed by the scene manager / not open
        if (!_asyncSceneRecords.TryGetValue(scene, out var sceneRecord))
            return;

        // TODO: Possibly delete this line
        scene.IsDisableInstead = false;

        // Debug.Log($"Unloading scene {scene.SceneField.SceneName} - {sceneRecord.State}");

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
                    // Debug.Log($"Unloading scene {scene.SceneField.SceneName}");
                    var op = SceneManager.UnloadSceneAsync(sceneRecord.SceneField);

                    if (op != null)
                        op.completed += _ => onCompletion?.Invoke();

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

    public void UnloadSceneAsync(LevelSectionSceneInfo levelSectionSceneInfo, Action onCompletion = null)
    {
        // Unload the section scene
        if (levelSectionSceneInfo.SectionScene != null)
        {
            // Create a scene unload field
            var sceneUnloadField = SceneUnloadField.Create(levelSectionSceneInfo.SectionScene, false);

            // Unload the scene
            UnloadSceneAsync(sceneUnloadField, onCompletion);
        }

        // Unload the section persistent data
        if (levelSectionSceneInfo.SectionPersistentData != null)
        {
            // Create a scene unload field that disables the scene instead of unloading it
            var sceneUnloadField = SceneUnloadField.Create(levelSectionSceneInfo.SectionPersistentData, true);

            // Unload the scene
            UnloadSceneAsync(sceneUnloadField, onCompletion);
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

        Debug.Log($"Forced management of scene {sceneField.SceneName}");
    }

    public void ForceManageScene(LevelSectionSceneInfo sceneLoaderInformation)
    {
        // Load the section scene
        if (sceneLoaderInformation.SectionScene != null)
        {
            ForceManageScene(sceneLoaderInformation.SectionScene);
            
            // Set the current scene info
            if (sceneLoaderInformation.SetActiveSceneToSectionScene)
                CurrentSceneInfo = sceneLoaderInformation;
                
        }

        // Load the section persistent data
        if (sceneLoaderInformation.SectionPersistentData != null)
            ForceManageScene(sceneLoaderInformation.SectionPersistentData);
    }

    public void LoadStartupScene(LevelStartupSceneInfo startupSceneInfo, MonoBehaviour coroutineRunner,
        Action<float> percentageCallback = null, Action onCompletion = null)
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
        coroutineRunner.StartCoroutine(
            LoadMultipleScenes(startupSceneInfo, coroutineRunner, percentageCallback, onCompletion)
        );
    }

    private IEnumerator LoadMultipleScenes(LevelStartupSceneInfo startupSceneInfo, MonoBehaviour coroutineRunner,
        Action<float> percentageCallback, Action onCompletion)
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

        // Load the player data scene
        if (startupSceneInfo.PlayerDataScene != null && startupSceneInfo.PlayerDataScene.SceneName.Trim() != "")
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
        
        // Load all the startup sections
        foreach (var section in startupSceneInfo.StartupSections)
        {
            // Load the section scene
            if (section.SectionScene != null && section.SectionScene.SceneName.Trim() != "")
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
            if (section.SectionPersistentData != null && section.SectionPersistentData.SceneName.Trim() != "")
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

        // Set all the load operations to allow scene activation
        foreach (var operation in loadOperations)
            operation.allowSceneActivation = true;

        var emptyScene = SceneManager.CreateScene("Empty");

        // // Unload all the currently loaded scenes asynchronously,
        // // but don't remove them from the hierarchy yet
        // foreach (var scene in scenesToUnload)
        // {
        //     var operation = SceneManager.UnloadSceneAsync(scene.name);
        //
        //     // Set the scene to be activated
        //     operation.allowSceneActivation = true;
        // }

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

        // Invoke the on completion action
        onCompletion?.Invoke();

        Debug.Log($"All Operations Done! - {startupSceneInfo.ActiveScene.SceneName}");
    }

    public void LoadMultipleScenesAsynchronously(
        SceneLoaderInformation loaderInformation,
        MonoBehaviour coroutineRunner,
        Action<float> percentageCallback,
        Action onCompletion)
    {
        // Return if the coroutine runner is null
        if (coroutineRunner == null)
        {
            Debug.LogError("Coroutine runner is null!");
            return;
        }

        // Return if the startup scene info is null
        if (loaderInformation == null)
        {
            Debug.LogError("Loader Information is null!");
            return;
        }

        // Start the coroutine
        coroutineRunner.StartCoroutine(
            LoadMultipleScenes(loaderInformation, coroutineRunner, percentageCallback, onCompletion)
        );
    }

    private IEnumerator LoadMultipleScenes(
        SceneLoaderInformation loaderInformation, MonoBehaviour coroutineRunner,
        Action<float> percentageCallback, Action onCompletion
    )
    {
        // Create a hash set to store all the load operations
        HashSet<AsyncOperation> loadOperations = new();

        foreach (var sectionSceneInfo in loaderInformation.SectionsToLoad)
        {
            // If the current section's scene or persistent data scene is not null,
            if (sectionSceneInfo != null)
            {
                var hasSectionScene =
                    _asyncSceneRecords.TryGetValue(sectionSceneInfo.SectionScene, out var sectionSceneRecord);
                var hasPersistentDataScene = _asyncSceneRecords.TryGetValue(sectionSceneInfo.SectionPersistentData,
                    out var persistentDataSceneRecord);

                var isSectionUnloadComplete = true;
                var isPersistentUnloadComplete = true;

                // If the scene is already loaded, unload it and wait for the operation to be complete
                if (hasSectionScene && sectionSceneRecord.State == AsyncSceneState.Loaded)
                {
                    // Set the flag to false
                    isSectionUnloadComplete = false;

                    // Create a scene unload field
                    var sceneUnloadField = SceneUnloadField.Create(sectionSceneInfo.SectionScene, false);

                    // Unload the scene
                    UnloadSceneAsync(sceneUnloadField, () => isSectionUnloadComplete = true);
                }

                // If the scene is already loaded, unload it and wait for the operation to be complete
                if (hasPersistentDataScene && persistentDataSceneRecord.State == AsyncSceneState.Loaded)
                {
                    // Set the flag to false
                    isPersistentUnloadComplete = false;

                    // Create a scene unload field
                    var sceneUnloadField = SceneUnloadField.Create(sectionSceneInfo.SectionPersistentData, false);

                    // Unload the scene
                    UnloadSceneAsync(sceneUnloadField, () => isPersistentUnloadComplete = true);
                }

                yield return new WaitUntil(() => isSectionUnloadComplete && isPersistentUnloadComplete);
            }
        }

        // Load all the sections
        foreach (var section in loaderInformation.SectionsToLoad)
        {
            // Load the section scene
            if (section.SectionScene != null && !string.IsNullOrEmpty(section.SectionScene.SceneName))
            {
                var operation = LoadSceneAsync(section.SectionScene);

                if (operation == null)
                {
                    Debug.LogError($"Operation for {section.SectionScene.SceneName} is null!");
                    continue;
                }

                // Set the scene to not be activated
                operation.allowSceneActivation = false;

                // Add the operation to the hash set
                loadOperations.Add(operation);

                Debug.Log($"Loading scene: {section.SectionScene.SceneName}");

                // Add the scene to the hash set
                _scenesThatNeedToLoadFromDisk.Add(section.SectionScene.SceneName);
            }

            // Load the section persistent data
            if (section.SectionPersistentData != null && !string.IsNullOrEmpty(section.SectionPersistentData.SceneName))
            {
                var operation = LoadSceneAsync(section.SectionPersistentData);

                if (operation == null)
                {
                    Debug.LogError($"Operation for {section.SectionPersistentData.SceneName} is null!");
                    continue;
                }

                // Set the scene to not be activated
                operation.allowSceneActivation = false;

                // Add the operation to the hash set
                loadOperations.Add(operation);

                Debug.Log($"Loading scene: {section.SectionPersistentData.SceneName}");

                // Add the scene to the hash set
                _scenesThatNeedToLoadFromDisk.Add(section.SectionPersistentData.SceneName);
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

        // Set all the load operations to allow scene activation
        foreach (var operation in loadOperations)
            operation.allowSceneActivation = true;
        
        // Wait for a frame
        yield return null;

        // Find the first active scene in the scenes to load
        foreach (var sceneInfo in loaderInformation.SectionsToLoad)
        {
            // Continue if not setting the active scene
            if (!sceneInfo.SetActiveSceneToSectionScene)
                continue;

            // Find the actual scene
            var activeScene = SceneManager.GetSceneByName(sceneInfo.SectionScene);

            // If the scene is not valid, continue
            if (!activeScene.IsValid())
                continue;

            Debug.Log($"Setting active scene to {activeScene.name}");

            // Wait until the scene is loaded
            yield return new WaitUntil(() => activeScene.isLoaded);
            
            // Set the active scene
            SceneManager.SetActiveScene(activeScene);
            break;
        }

        // Unload all the currently loaded scenes asynchronously,
        // but don't remove them from the hierarchy yet
        foreach (var section in loaderInformation.SectionsToUnload)
            UnloadSceneAsync(section);

        onCompletion?.Invoke();
    }

    public void DebugLoadSceneSynchronous(SceneLoaderInformation loaderInformation)
    {
        // Unload all the scenes to unload asynchronously
        foreach (var scene in loaderInformation.SectionsToUnload)
            UnloadSceneAsync(scene);

        // Clear the debug loaded scenes
        _debugLoadedScenes.Clear();

        // Add all the scenes to load to the debug loaded scenes
        foreach (var scene in loaderInformation.SectionsToLoad)
            _debugLoadedScenes.Add(scene.SectionScene.SceneName);

        // Load all the scenes to load synchronously
        foreach (var scene in loaderInformation.SectionsToLoad)
            LoadSceneSynchronous(scene);

        // Start the coroutine to set the active scene
        DebugManagerHelper.Instance.StartCoroutine(DebugLoadActiveScene(loaderInformation));
    }

    private static IEnumerator DebugLoadActiveScene(SceneLoaderInformation loaderInformation)
    {
        // Wait 1 frame
        yield return null;

        // Set the active scene
        foreach (var scene in loaderInformation.SectionsToLoad)
        {
            // Continue if not setting the active scene
            if (!scene.SetActiveSceneToSectionScene)
                continue;

            var activeScene = SceneManager.GetSceneByName(scene.SectionScene);

            // If the scene is not valid, continue
            if (!activeScene.IsValid())
                continue;

            // Set the active scene
            SceneManager.SetActiveScene(activeScene);

            break;
        }
    }

    #endregion

    // TODO: Remove
    public PlayerMovementType _movementType;

    private void SetPlayerMovementType(PlayerMovementType movementType)
    {
        _movementType = movementType;

        // If the player instance is not null
        if (Player.Instance != null)
        {
            // Get the player movement
            var playerMovement = Player.Instance.PlayerController as PlayerMovementV2;

            // If the player movement is null, return
            if (playerMovement == null)
                return;

            // Set the movement type
            playerMovement.ApplyMovementTypeSettings(movementType);
        }
    }

    public string GetDebugText()
    {
        return $"Managed Scenes: {string.Join(", ", _asyncSceneRecords.Keys)}";
    }

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