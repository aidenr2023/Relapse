using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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

    private readonly ConcurrentDictionary<string, Scene> _managedScenes = new();

    private readonly Dictionary<string, IEnumerable<GameObject>> _disabledScenes = new();

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

        if (sceneName == null)
            return;

        // If the scene already exists in the managed scenes, return
        if (_managedScenes.ContainsKey(sceneName))
        {
            // If the scene is disabled, enable it
            if (!_disabledScenes.TryGetValue(sceneName, out var disabledSceneObjects))
                return;

            Debug.Log($"Re-Enabling Scene: {sceneName}");

            foreach (var sceneObject in disabledSceneObjects)
                sceneObject.SetActive(true);

            // Remove the scene from the disabled scenes
            _disabledScenes.Remove(sceneName);

            return;
        }

        // Start to async load the scene
        var asyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        // Check if the async operation is null
        if (asyncOperation == null)
        {
            // Log an error
            Debug.LogError($"Failed to load scene: {sceneName}");
            return;
        }

        // Set the priority of the async operation
        asyncOperation.priority = priority;

        var scene = SceneManager.GetSceneByName(sceneName);

        Debug.Log($"Scene: {scene.name} is loaded: {scene.isLoaded}");

        // Add the scene to the managed scenes
        _managedScenes.TryAdd(sceneName, scene);
    }

    public void LoadScene(SceneLoadInformation.SceneLoadInfoPriority scene)
    {
        if (scene == null)
            return;

        LoadScene(scene.SceneName, scene.Priority);
    }

    public void LoadSceneNoAsync(string sceneName)
    {
        // Check if the scene name is null
        if (sceneName == null)
            return;

        // Check if the scene is already loaded
        if (_managedScenes.ContainsKey(sceneName))
            return;

        // Load the scene
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);

        // Add the scene to the managed scenes
        _managedScenes.TryAdd(sceneName, SceneManager.GetSceneByName(sceneName));
    }

    public void UnloadScene(string sceneName)
    {
        if (sceneName == null)
            return;

        // Check if the scene is not in the managed scenes
        if (!_managedScenes.ContainsKey(sceneName))
            return;

        // Check if the scene has a value
        if (!_managedScenes.TryGetValue(sceneName, out var scene))
            return;

        // Return if the scene is already disabled
        if (_disabledScenes.ContainsKey(sceneName))
            return;

        // // Unload the scene
        // SceneManager.UnloadSceneAsync(scene);
        //
        // // Remove the scene from the managed scenes
        // _managedScenes.TryRemove(sceneName, out _);

        var sceneObjects = scene.GetRootGameObjects();

        // Get all the currently active objects in the scene
        var activeObjects = new List<GameObject>();

        // Disable the scene by disabling all its game objects
        foreach (var sceneObject in sceneObjects)
        {
            // Continue if the scene object is not active
            if (!sceneObject.activeSelf)
                continue;

            // Disable the scene object
            sceneObject.SetActive(false);

            // Add the scene object to the active objects
            activeObjects.Add(sceneObject);
        }

        // Add this scene to the disabled scenes
        _disabledScenes.Add(sceneName, activeObjects);
    }

    public void LoadScenesExclusive(SceneLoadInformation sceneLoadInformation)
    {
        if (sceneLoadInformation == null)
            return;

        // Create a hash set to store all the scenes that need to be unloaded
        var scenesToUnload = new HashSet<string>(_managedScenes.Keys);

        // For each scene in the scene load information, remove the scene from the scenes to unload
        foreach (var scene in sceneLoadInformation.Scenes)
            scenesToUnload.Remove(scene.SceneName);

        // Unload all the scenes that need to be unloaded
        foreach (var scene in scenesToUnload)
            UnloadScene(scene);

        // Load all the scenes in the scene load information
        foreach (var scene in sceneLoadInformation.Scenes)
            LoadScene(scene);
    }

    private Scene[] GetAllScenes()
    {
        // Get the scene count
        var sceneCount = SceneManager.sceneCount;

        // Get all scenes
        var scenes = new Scene[sceneCount];
        for (var i = 0; i < sceneCount; i++)
            scenes[i] = SceneManager.GetSceneAt(i);

        return scenes;
    }
}