using UnityEngine;

public class SceneLoadTrigger : MonoBehaviour
{
    [SerializeField] private SceneLoaderInformation sceneLoader;

    private void OnTriggerEnter(Collider other)
    {
        // Return if the other collider is not the player
        if (!other.CompareTag("Player"))
            return;

        // Load the scenes via the scene manager
        AsyncSceneManager.Instance.LoadSceneAsync(sceneLoader);
    }

}