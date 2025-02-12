using System;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class HealthOverlayUI : MonoBehaviour
{
    public static HealthOverlayUI Instance { get; private set; }

    private CanvasGroup _canvasGroup;

    public CanvasGroup CanvasGroup => _canvasGroup;

    private void Awake()
    {
        // Since this is part of the game UI, we want to make sure there is only one instance of this
        // If the instance is not null and not this, destroy this
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        // Set the instance to this
        Instance = this;

        // Get the CanvasGroup component
        _canvasGroup = GetComponent<CanvasGroup>();

        // Set the alpha to 0
        _canvasGroup.alpha = 0;
    }

    private void OnDestroy()
    {
        // Set the instance to null
        if (Instance == this)
            Instance = null;
    }
}