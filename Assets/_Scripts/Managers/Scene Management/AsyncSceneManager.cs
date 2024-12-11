using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AsyncSceneManager : MonoBehaviour
{
    public static AsyncSceneManager Instance { get; private set; }

    #region Private Fields

    private readonly Dictionary<string, AsyncSceneRecord> _asyncSceneRecords = new();

    private readonly Dictionary<string, GameObject[]> _disabledSceneObjects = new();

    #endregion

    private void Awake()
    {
        // Set the instance
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void LoadSceneAsync(SceneField scene)
    {
        // If there is already a record for the scene
        if (_asyncSceneRecords.TryGetValue(scene, out var sceneRecord))
        {
            switch (sceneRecord.State)
            {
                // If the scene is currently loading, return
                case AsyncSceneState.Loading:
                    return;

                // If the scene is already loaded, return
                case AsyncSceneState.Loaded:
                    return;

                // If the scene is canceled, set the async operation to finish loading
                case AsyncSceneState.Canceled:
                    sceneRecord.State = AsyncSceneState.Loading;

                    if (sceneRecord.Operation != null)
                        sceneRecord.Operation.allowSceneActivation = true;
                    return;

                // If the scene is unloaded, remove it from the dictionary and call the function again
                case AsyncSceneState.Unloaded:
                    _asyncSceneRecords.Remove(scene);
                    LoadSceneAsync(scene);
                    return;

                // If the scene is disabled, enable it
                case AsyncSceneState.Disabled:

                    // Enable the scene
                    EnableScene(sceneRecord.Scene);

                    // Set the scene state to loaded
                    sceneRecord.State = AsyncSceneState.Loaded;

                    return;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        // Add a tooltip to the scene field
        JournalTooltipManager.Instance.AddTooltip($"Loading {scene.SceneName}...");

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

    private void DisableScene(Scene scene)
    {
        // Get all the root game objects in the scene
        var rootGameObjects = scene.GetRootGameObjects();

        var objectsToTrack = rootGameObjects
            .Where(n => isActiveAndEnabled)
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

    private void LoadSceneSynchronous(SceneField scene)
    {
        // If the scene is already loaded, return
        if (_asyncSceneRecords.TryGetValue(scene, out var sceneRecord) && sceneRecord.State == AsyncSceneState.Loaded)
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