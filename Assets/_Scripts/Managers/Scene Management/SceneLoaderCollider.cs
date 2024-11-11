using UnityEngine;

public class SceneLoaderCollider : MonoBehaviour
{
    [SerializeField] private string[] scenesToLoad;

    private void OnTriggerEnter(Collider other)
    {
        // Load the scenes if the player enters the collider
        if (other.CompareTag("Player"))
            AsyncSceneLoader.Instance.LoadScenesExclusive(scenesToLoad);
    }
}