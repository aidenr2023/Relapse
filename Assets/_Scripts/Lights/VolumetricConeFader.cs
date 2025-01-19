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
    [Header("Fading Settings")]
    [Tooltip("Transform to measure distance from (player or camera). If left null, will use Camera.main.")]
    public Transform player;

    [Tooltip("Distance at which the cone is fully opaque.")]
    public float fadeStartDistance = 5f;

    [Tooltip("Distance at which the cone is fully invisible.")]
    public float fadeEndDistance = 1f;

    // We'll store a reference to the per-instance material and its initial color.
    private Material _materialInstance;
    private Color _initialColor;

    void Start()
    {
        // Grab the MeshRenderer and create an instance of the material
        var rend = GetComponent<Renderer>();
        if (rend)
        {
            // .material creates a unique material instance for this renderer
            _materialInstance = rend.material;
            _initialColor = _materialInstance.GetColor("_Color");
        }
        else
        {
            Debug.LogWarning("VolumetricConeFader: No Renderer found on this GameObject.");
        }

        // If no player assigned, fallback to main camera
        if (player == null && Camera.main != null)
        {
            player = Camera.main.transform;
        }
    }

    void Update()
    {
        if (_materialInstance == null || player == null) return;

        // Distance from the cone's pivot to the player/camera
        float distance = Vector3.Distance(transform.position, player.position);

        // If distance >= fadeStartDistance => alphaFactor = 1 (fully visible)
        // If distance <= fadeEndDistance => alphaFactor = 0 (fully invisible)
        // Between them => linear interpolation
        float alphaFactor = Mathf.InverseLerp(fadeEndDistance, fadeStartDistance, distance);

        // Apply to the material color
        // e.g. if _initialColor has alpha=0.5, final alpha = 0.5 * alphaFactor
        Color c = _initialColor;
        c.a *= alphaFactor;
        _materialInstance.SetColor("_Color", c);
    }
}
