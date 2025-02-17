using System;
using UnityEngine;

public class InteractableManager : MonoBehaviour
{
    public static InteractableManager Instance { get; private set; }
    
    #region Serialized Fields
    
    [SerializeField] private Material outlineMaterial;
    
    #endregion
    
    #region Getters
    
    public Material OutlineMaterial => outlineMaterial;
    
    #endregion

    private void Awake()
    {
        // Destroy the instance if it already exists
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        // Set the instance
        Instance = this;
    }
    
    private void OnDestroy()
    {
        // Reset the instance
        if (Instance == this)
            Instance = null;
    }
}