using System;
using UnityEngine;

public class PersistentGlobalObject : MonoBehaviour
{
    private static PersistentGlobalObject _instance;
    
    private void Awake()
    {
        // If the instance is not null and the instance is not this, destroy this object
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        // Set the instance to this
        _instance = this;
    }
}