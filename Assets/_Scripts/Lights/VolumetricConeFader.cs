using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Fades out a 'volumetric' cone as the player (or camera) gets close.
/// Attach to the cone mesh GameObject that has the VolumetricFogCone material.
/// </summary>
[RequireComponent(typeof(Renderer))]
public class VolumetricConeFader : MonoBehaviour
{
    private static readonly int ColorShaderID = Shader.PropertyToID("_Color");
    private static readonly int OpacityShaderID = Shader.PropertyToID("_Opacity");

    [Header("Fading Settings")] [Tooltip("Distance at which the cone is fully opaque.")]
    public float fadeStartDistance = 5f;

    [Tooltip("Distance at which the cone is fully invisible.")]
    public float fadeEndDistance = 1f;

    // We'll store a reference to the per-instance material and its initial color.
    private Material _materialInstance;

    // private Color _initialColor;
    private float _initialOpacity;

    private void Start()
    {
        // Grab the MeshRenderer and create an instance of the material
        var materialRenderer = GetComponent<Renderer>();
        if (materialRenderer)
        {
            // material creates a unique material instance for this renderer
            _materialInstance = materialRenderer.material;
            // _initialColor = _materialInstance.GetColor(ColorShaderID);
            _initialOpacity = _materialInstance.GetFloat(OpacityShaderID);
        }
        else
            Debug.LogWarning("VolumetricConeFader: No Renderer found on this GameObject.");
    }

    private void Update()
    {
        var player = Player.Instance;

        if (_materialInstance == null || player == null)
            return;

        // Distance from the cone's pivot to the player/camera
        var distance = Vector3.Distance(transform.position, player.transform.position);

        // If distance >= fadeStartDistance => alphaFactor = 1 (fully visible)
        // If distance <= fadeEndDistance => alphaFactor = 0 (fully invisible)
        // Between them => linear interpolation
        var alphaFactor = Mathf.InverseLerp(fadeEndDistance, fadeStartDistance, distance);

        var newOpacity = Mathf.Lerp(0, _initialOpacity, alphaFactor);
        
        // Set the new opacity value
        _materialInstance.SetFloat(OpacityShaderID, newOpacity);
    }
}