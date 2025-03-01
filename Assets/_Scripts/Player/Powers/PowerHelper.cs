using System;
using System.Collections.Generic;
using UnityEngine;

public class PowerHelper : MonoBehaviour
{
    public static PowerHelper Instance { get; private set; }
    
    [SerializeField] private PowerScriptableObject[] powers;
    
    public IReadOnlyCollection<PowerScriptableObject> Powers => powers;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        // Clear the instance
        if (Instance == this)
            Instance = null;
    }
}