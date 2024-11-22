using UnityEngine;

public class Journal : MonoBehaviour
{
    public static Journal Instance { get; private set; }

    private void Awake()
    {
        // Ensure only one instance of the Journal exists
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        // Set the instance to this object
        Instance = this;

        // Don't destroy the Journal when changing scenes
        DontDestroyOnLoad(gameObject);
    }


}