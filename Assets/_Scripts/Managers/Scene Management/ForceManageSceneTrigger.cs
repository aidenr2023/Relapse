using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ForceManageSceneTrigger : MonoBehaviour
{
    [SerializeField] private LevelSectionSceneInfo[] scenesToManage;

    [SerializeField] private LevelSectionSceneInfo[] immediateLoadScenes;

    private bool _hasLoadedScenes;

    private void Start()
    {
        // Manage the scenes
        ManageScenes();

        // Load the immediate scenes
        LoadImmediateScenes();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Return if the other collider is not the player
        if (!other.CompareTag("Player"))
            return;

        ManageScenes();
    }

    private void ManageScenes()
    {
        // Load the scenes via the scene manager
        foreach (var scene in scenesToManage)
            AsyncSceneManager.Instance.ForceManageScene(scene);
    }

    private void LoadImmediateScenes()
    {
        // Return if the immediate load scenes are null
        if (immediateLoadScenes == null)
            return;

        // Return if the immediate load scenes have already been loaded
        if (_hasLoadedScenes)
            return;

        // Return if not in-editor
        if (!Application.isEditor)
            return;

        // Return if this game object's scene is not the active scene
        if (SceneManager.GetActiveScene() != gameObject.scene)
            return;

        // Load the scenes via the scene manager
        foreach (var scene in immediateLoadScenes)
            AsyncSceneManager.Instance.LoadSceneSynchronous(scene);

        _hasLoadedScenes = true;
    }
}