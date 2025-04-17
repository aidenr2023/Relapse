using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ForceSceneLoadHelper : MonoBehaviour
{
    [SerializeField] private string sceneName;
    
    public void ForceChangeScene()
    {
        SceneManager.LoadScene(sceneName);
    }

}