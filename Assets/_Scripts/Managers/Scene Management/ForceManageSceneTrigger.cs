using UnityEngine;

public class ForceManageSceneTrigger : MonoBehaviour
{
    [SerializeField] private SceneField[] scenesToManage;

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