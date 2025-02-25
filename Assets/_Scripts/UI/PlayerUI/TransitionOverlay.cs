using System;
using UnityEngine;

public class TransitionOverlay : MonoBehaviour
{
    public static TransitionOverlay Instance { get; private set; }

    [SerializeField] private CanvasGroup canvasGroup;

    private void Awake()
    {
        // if the instance is not null and is not this instance
        if (Instance != null && Instance != this)
        {
            // Destroy this instance
            Destroy(gameObject);
            return;
        }

        // Initialize the instance
        Instance = this;
    }

    private void OnDestroy()
    {
        // If the instance is this instance
        // Set the instance to null
        if (Instance == this)
            Instance = null;
    }


    public void SetOpacity(float opacity)
    {
        canvasGroup.alpha = opacity;
    }
}