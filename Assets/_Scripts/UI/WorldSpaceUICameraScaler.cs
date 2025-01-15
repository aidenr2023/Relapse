using System;
using Cinemachine;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class WorldSpaceUICameraScaler : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera vCam;
    [SerializeField] private Canvas canvas;

    [SerializeField] private Vector3 originalScale;
    [SerializeField, Readonly] private float originalFov;

    // Canvas scaling based on screen size
    [Space, SerializeField] private Vector2 targetResolution = new(1920, 1080);

    private float _fovScaleFactor;
    private float _resolutionScaleFactor;

    private void Awake()
    {
        originalScale = transform.localScale;
        originalFov = vCam.m_Lens.FieldOfView;
    }

    private void LateUpdate()
    {
        SetScale();
    }

    private void FixedUpdate()
    {
        SetScale();
    }

    private void SetScaleBasedOnFov()
    {
        // Return if the virtual camera is null
        // Return if the canvas is null
        if (vCam == null || canvas == null)
        {
            _fovScaleFactor = 1;
            return;
        }

        // Get the current field of view
        var currentFov = vCam.m_Lens.FieldOfView;

        // Get the scale factor
        var scaleFactor = currentFov / originalFov;

        if (Math.Abs(scaleFactor - 1) < 0.0001f)
            scaleFactor = 1;

        // Set the scale of the canvas
        _fovScaleFactor = scaleFactor;
    }

    private void SetScaleBasedOnScreenResolution()
    {
        // Return if the canvas is null
        if (canvas == null)
            return;

        // Get the current resolution
        var currentResolution = new Vector2(Screen.width, Screen.height);

        // Get the aspect ratio of the current resolution
        var currentAspectRatio = currentResolution.x / currentResolution.y;

        // Get the aspect ratio of the target resolution
        var targetAspectRatio = targetResolution.x / targetResolution.y;

        // Get the scale factor
        var scaleFactor = currentAspectRatio / targetAspectRatio;

        if (Math.Abs(scaleFactor - 1) < 0.0001f)
            scaleFactor = 1;

        // Set the scale of the canvas
        _resolutionScaleFactor = scaleFactor;
    }

    private void SetScale()
    {
        SetScaleBasedOnFov();
        SetScaleBasedOnScreenResolution();

        transform.localScale = originalScale * (_fovScaleFactor * _resolutionScaleFactor);
    }
}