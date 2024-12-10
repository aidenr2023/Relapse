using System;
using UnityEngine;

public class ForceManageSceneTrigger : MonoBehaviour
{
    [SerializeField] private LevelSectionSceneInfo[] scenesToManage;

    [SerializeField] private LevelSectionSceneInfo[] immediateLoadScenes;

    private void Start()
    {
        // Load the scenes via the scene manager
        foreach (var scene in immediateLoadScenes)
            AsyncSceneManager.Instance.LoadSceneSynchronous(scene);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Return if the other collider is not the player
        if (!other.CompareTag("Player"))
            return;

        // Load the scenes via the scene manager
        foreach (var scene in scenesToManage)
            AsyncSceneManager.Instance.ForceManageScene(scene);
    }
}