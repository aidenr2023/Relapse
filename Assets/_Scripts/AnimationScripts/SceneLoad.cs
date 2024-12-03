using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLoad : MonoBehaviour
{
    // The name of the scene to load
    [SerializeField] private string sceneName;

    // The time to wait before loading the scene
    [SerializeField] private float waitTime = 3f;

    // Start is called before the first frame update
    void Start()
    {
        // Start the coroutine to load the scene after the wait time
        //StartCoroutine(LoadScene());
    }

    // Coroutine to load the scene after the wait time
    private void LoadScene()
    {
        // Wait for the specified time
        //yield return new WaitForSeconds(waitTime);

        // Load the scene with the specified name
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
    //on trigger enter load scene
    private void OnTriggerEnter(Collider other)
    {
        // Load the scene with the specified name
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}
