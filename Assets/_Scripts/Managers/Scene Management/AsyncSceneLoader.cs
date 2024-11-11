using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// The current scene is NOT added to the scene manager, so it will not be unloaded.
/// This is because Unity cannot unload the base scene.
/// </summary>
public class AsyncSceneLoader : MonoBehaviour
{
    public static AsyncSceneLoader Instance { get; private set; }

    [SerializeField] private SceneLoadInformation scenesToLoadOnAwake;

    private readonly Dictionary<string, AsyncOperation> _asyncOperations = new();

    private void Awake()
    {
        // Set the instance to this
        Instance = this;

        // Load the scenes on awake
        if (scenesToLoadOnAwake != null)
            foreach (var scene in scenesToLoadOnAwake.Scenes)
                LoadSceneNoAsync(scene.SceneName);
    }

    public void LoadScene(string sceneName, int priority = 0)
    {
        Debug.Log($"Loading Scene!: {sceneName} w/ priority: {priority}");

        // Return if the scene is already loaded
        if (_asyncOperations.ContainsKey(sceneName))
            return;

        // Start loading the scene
        var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        // Set the priority of the scene's loading operation
        if (op != null)
            op.priority = priority;

        // Add the scene to the dictionary
        _asyncOperations.Add(sceneName, op);
    }

    public void LoadScene(SceneLoadInformation.SceneLoadInfoPriority scene)
    {
        if (scene == null)
            return;

        LoadScene(scene.SceneName, scene.Priority);
    }

    public void LoadSceneNoAsync(string sceneName)
    {
        // Return if the scene is already loaded
        if (_asyncOperations.ContainsKey(sceneName))
            return;

        // Start loading the scene
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);

        // Add the scene to the dictionary
        _asyncOperations.Add(sceneName, null);
    }

    public void UnloadScene(string sceneName)
    {
        if (sceneName == null)
            return;

        // Return if the scene is not loaded
        if (!_asyncOperations.TryGetValue(sceneName, out var op))
            return;

        // // Stop the scene from loading
        // if (op != null)
        //     op.allowSceneActivation = false;

        SceneManager.UnloadSceneAsync(sceneName);

        _asyncOperations.Remove(sceneName);
    }

    public void LoadScenesExclusive(SceneLoadInformation sceneLoadInformation)
    {
        if (sceneLoadInformation == null)
            return;

        // Keep track of the scenes that need to be loaded
        var scenesToLoad = new HashSet<SceneLoadInformation.SceneLoadInfoPriority>(sceneLoadInformation.Scenes);

        var scenesToUnload = new HashSet<string>(_asyncOperations.Keys);

        // Remove all scenes that need to be loaded from the scenes to unload
        foreach (var scene in scenesToLoad)
            scenesToUnload.Remove(scene.SceneName);

        // Unload all scenes
        foreach (var scene in scenesToUnload)
            UnloadScene(scene);

        // Load all new scenes
        foreach (var scene in scenesToLoad)
        {
            // Continue if the scene is already loaded
            if (_asyncOperations.ContainsKey(scene.SceneName))
                continue;

            LoadScene(scene);
        }
    }
}