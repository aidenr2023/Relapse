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
        // Set the instance to this
        Instance = this;

        // Get the CanvasGroup component
        _canvasGroup = GetComponent<CanvasGroup>();
        
        // Set the alpha to 0
        _canvasGroup.alpha = 0;
    }
}