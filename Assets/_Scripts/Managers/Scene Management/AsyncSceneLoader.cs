using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// The current scene is NOT added to the scene manager, so it will not be unloaded.
/// </summary>
public class AsyncSceneLoader : MonoBehaviour
{
    public static AsyncSceneLoader Instance { get; private set; }

    private readonly Dictionary<string, AsyncOperation> _asyncOperations = new();

    private void Awake()
    {
        // Set the instance to this
        Instance = this;
    }

    public void LoadScene(string sceneName)
    {
        Debug.Log($"Loading Scene!: {sceneName}");

        // Return if the scene is already loaded
        if (_asyncOperations.ContainsKey(sceneName))
            return;

        // Start loading the scene
        var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        // Add the scene to the dictionary
        _asyncOperations.Add(sceneName, op);
    }

    public void UnloadScene(string sceneName)
    {
        if (sceneName == null)
            return;

        // Return if the scene is not loaded
        if (!_asyncOperations.TryGetValue(sceneName, out var op))
            return;

        // Stop the scene from loading
        if (op != null)
            op.allowSceneActivation = false;

        SceneManager.UnloadSceneAsync(sceneName);

        _asyncOperations.Remove(sceneName);
    }

    public void LoadScenesExclusive(string[] scenes)
    {
        // Keep track of the scenes that need to be loaded
        var scenesToLoad = new HashSet<string>(scenes);

        foreach (var sceneName in scenes)
        {
            // Continue if the scene is already loaded
            if (_asyncOperations.ContainsKey(sceneName))
                continue;

            // Add the scene to the scenes to load
            scenesToLoad.Add(sceneName);
        }

        var scenesToUnload = new HashSet<string>(_asyncOperations.Keys);

        // Remove all scenes that need to be loaded from the scenes to unload
        foreach (var sceneName in scenesToLoad)
            scenesToUnload.Remove(sceneName);

        // Unload all scenes
        foreach (var sceneName in scenesToUnload)
            UnloadScene(sceneName);

        // Load all new scenes
        foreach (var sceneName in scenesToLoad)
            LoadScene(sceneName);
    }
}