using System;
using UnityEngine;

public class SceneLoaderCollider : MonoBehaviour
{
    [SerializeField] private SceneLoadInformation scenesToLoad;

    private void Awake()
    {
        // Get all renderers so the collider is visible in the editor
        var renderers = GetComponentsInChildren<Renderer>();

        // Set the renderers to be invisible
        foreach (var cRenderer in renderers)
            GetComponent<Renderer>().enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Load the scenes if the player enters the collider
        if (other.CompareTag("Player"))
            AsyncSceneLoader.Instance.LoadScenesExclusive(scenesToLoad);
    }
}