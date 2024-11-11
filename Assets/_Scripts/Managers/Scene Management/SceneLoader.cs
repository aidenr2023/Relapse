using UnityEngine;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private SceneLoadInformation scenesToLoad;

    public void LoadScene()
    {
        AsyncSceneLoader.Instance.LoadScenesExclusive(scenesToLoad);
    }
}