using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiDissolver : MonoBehaviour
{
    private static readonly int DissolveStrengthProperty = Shader.PropertyToID("_DissolveStrength");
    private static readonly int DissolveBoolProperty = Shader.PropertyToID("_Dissolve");

    private const float DISSOLVE_IN_TARGET_STRENGTH = 0;
    private const float DISSOLVE_OUT_TARGET_STRENGTH = 1;

    #region Serialized Fields

    [Header("1 = fully dissolved, 0 = fully visible")]
    [SerializeField, Range(0, 1)] private float startingDissolveStrength = 1f;

    [SerializeField, Min(0)] private float dissolveInDuration = 3;
    [SerializeField, Min(0)] private float dissolveOutDuration = 3;

    [Header(
        "This script only works on renderers with ONE material.")]
    [SerializeField]
    private Renderer[] dissolveRenderers;

    #endregion

    private Coroutine _currentCoroutine;

    private void Awake()
    {
        // Create an instance of the materials so that the actual assets are not modified
        InstanceMaterials(dissolveRenderers);
        
        // Set the starting dissolve strength
        foreach (var cRenderer in dissolveRenderers)
            SetDissolveStrength(cRenderer, startingDissolveStrength);
    }

    private static void InstanceMaterials(Renderer[] renderers)
    {
        foreach (var renderer in renderers)
        {
            var materials = renderer.materials;
            for (var i = 0; i < materials.Length; i++)
            {
                var material = new Material(materials[i]);
                materials[i] = material;
            }

            renderer.materials = materials;
        }
    }

    private static IEnumerator SetDissolveStrengthCoroutine(Renderer[] renderers, float targetStrength, float duration)
    {
        // // Set the dissolve boolean
        // var useDissolve = targetStrength > 0 ? 1 : 0;
        // foreach (var renderer in renderers)
        //     renderer.sharedMaterial.SetInt(DissolveBoolProperty, useDissolve);
        
        // Clamp the strength to the range [0, 1]
        targetStrength = Mathf.Clamp01(targetStrength);

        // Get the starting time
        var startTime = Time.time;

        // Store the starting strengths of the renderers
        var startingStrengths = new Dictionary<Renderer, float>();

        // Get the float property ID for the dissolve strength
        foreach (var renderer in renderers)
            startingStrengths[renderer] = renderer.sharedMaterial.GetFloat(DissolveStrengthProperty);

        while (Time.time < startTime + duration)
        {
            // Calculate the elapsed time
            var elapsedTime = Time.time - startTime;

            // Inverse lerp the elapsed time
            var lerpValue = elapsedTime / duration;

            foreach (var renderer in renderers)
            {
                // Lerp the strength
                var newStrength = Mathf.Lerp(startingStrengths[renderer], targetStrength, lerpValue);

                // Set the dissolve strength for each renderer
                SetDissolveStrength(renderer, newStrength);
            }

            // Wait for the next frame
            yield return null;
        }

        foreach (var renderer in renderers)
            renderer.sharedMaterial.SetFloat(DissolveStrengthProperty, targetStrength);
    }

    public void SetDissolveStrength(float strength)
    {
        // Clamp the strength to the range [0, 1]
        strength = Mathf.Clamp01(strength);
        
        // Set the dissolve strength for each renderer
        foreach (var dissolveRenderer in dissolveRenderers)
            SetDissolveStrength(dissolveRenderer, strength);
        
    }
    
    private static void SetDissolveStrength(Renderer renderer, float strength)
    {
        // Clamp the strength to the range [0, 1]
        strength = Mathf.Clamp01(strength);

        // Set the dissolve strength for each renderer
        renderer.sharedMaterial.SetFloat(DissolveStrengthProperty, strength);
    }

    public void DissolveIn()
    {
        // Stop the current coroutine (if it exists)
        if (_currentCoroutine != null)
            StopCoroutine(_currentCoroutine);

        // Start the dissolve in coroutine
        _currentCoroutine = StartCoroutine(SetDissolveStrengthCoroutine(
            dissolveRenderers, DISSOLVE_IN_TARGET_STRENGTH,
            dissolveInDuration)
        );

        Debug.Log("DissolveIn");
    }

    public void DissolveOut()
    {
        // Stop the current coroutine (if it exists)
        if (_currentCoroutine != null)
            StopCoroutine(_currentCoroutine);

        // Start the dissolve out coroutine
        _currentCoroutine = StartCoroutine(SetDissolveStrengthCoroutine(
            dissolveRenderers, DISSOLVE_OUT_TARGET_STRENGTH,
            dissolveOutDuration)
        );

        Debug.Log("DissolveOut");
    }
}