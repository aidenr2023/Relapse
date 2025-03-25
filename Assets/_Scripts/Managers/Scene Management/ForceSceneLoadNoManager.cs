using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ForceSceneLoadNoManager : MonoBehaviour
{
    [SerializeField] private SceneField sceneToLoad;
    [SerializeField] private bool doOnEnable = false;

    private void OnEnable()
    {
        if (doOnEnable)
            LoadScene();
    }

    public void LoadScene()
    {
        // Unload all active scenes
        AsyncSceneManager.Instance.UnloadAllActiveScenes();
        
        // Load the scene
        SceneManager.LoadScene(sceneToLoad);
    }
}