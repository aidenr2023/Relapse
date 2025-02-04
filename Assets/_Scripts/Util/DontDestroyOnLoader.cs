using System;
using UnityEngine;

public class DontDestroyOnLoader : MonoBehaviour
{
    private void Awake()
    {
        // Set the parent to null
        transform.SetParent(null);
        
        // Don't destroy this object when loading a new scene
        DontDestroyOnLoad(gameObject);
    }
}