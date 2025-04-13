using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BossShield : MonoBehaviour
{
    private static readonly int FadePropertyID = Shader.PropertyToID("_Fade");
    private static readonly int ColorPropertyID = Shader.PropertyToID("_Color");

    #region Serialized Fields

    [SerializeField] private Collider shieldCollider;
    [SerializeField] private Renderer[] shieldRenderers;
    [SerializeField] private float fadeTime = .5f;

    [SerializeField, ColorUsage(true, true)]
    private Color hitColor = Color.white;

    [SerializeField] private AnimationCurve hitColorCurve;

    #endregion

    #region Private Fields

    private readonly HashSet<object> _shieldActivators = new();
    private Coroutine _fadeCoroutine;
    private Coroutine _hitColorCoroutine;

    private Color _initialColor;

    #endregion

    #region Initialization Functions

    private void Awake()
    {
        // Deactivate the shield collider at the start
        _shieldActivators.Clear();
        ForceShieldCollider(false);
        ForceFade(0);

        // Initialize the material
        InitializeMaterial();
    }

    #endregion

    private void InitializeMaterial()
    {
        // Create an instance of the material so that the original isn't affected
        foreach (var cRenderer in shieldRenderers)
        {
            // _shieldMaterials[cRenderer] = cRenderer.material;
            // cRenderer.material = _shieldMaterials[cRenderer];

            cRenderer.material = new Material(cRenderer.sharedMaterial);
        }

        // Set the initial color
        _initialColor = shieldRenderers.First().sharedMaterial.GetColor(ColorPropertyID);
    }

    public void ActivateShield(object activator)
    {
        var previousActivatorCount = _shieldActivators.Count;

        // Add the activator to the set
        _shieldActivators.Add(activator);

        SetShieldCollider(previousActivatorCount);
    }

    public void DeactivateShield(object activator)
    {
        var previousActivatorCount = _shieldActivators.Count;

        // Remove the activator from the set
        _shieldActivators.Remove(activator);

        SetShieldCollider(previousActivatorCount);
    }

    public void ActivateShieldInspector(UnityEngine.Object activator) => ActivateShield(activator);

    public void DeactivateShieldInspector(UnityEngine.Object activator) => DeactivateShield(activator);

    private void SetShieldCollider(int previousActivatorCount)
    {
        var isActive = _shieldActivators.Count > 0;

        // Enable the shield collider
        ForceShieldCollider(isActive);

        // Start the fade in
        if (isActive && previousActivatorCount <= 0)
            StartShieldFade(1, fadeTime);

        // Start the fade out
        else if (!isActive && previousActivatorCount > 0)
            StartShieldFade(0, fadeTime);
    }

    private void StartShieldFade(float targetValue, float duration)
    {
        // Stop the current fade coroutine if it exists
        if (_fadeCoroutine != null)
            StopCoroutine(_fadeCoroutine);

        _fadeCoroutine = StartCoroutine(ShieldFade(targetValue, duration));
    }

    private IEnumerator ShieldFade(float targetValue, float duration)
    {
        // Get the start value of the fade property
        var startValue = shieldRenderers.First().sharedMaterial.GetFloat(FadePropertyID);

        var startTime = Time.time;

        while (Time.time < startTime + duration)
        {
            // Calculate the current value of the fade property
            var t = (Time.time - startTime) / duration;
            var currentValue = Mathf.Lerp(startValue, targetValue, t);

            // Set the fade property
            ForceFade(currentValue);

            yield return null;
        }

        // Set the fade property
        ForceFade(targetValue);
    }

    public void StartHitColorCoroutine()
    {
        // Stop the current hit color coroutine if it exists}
        if (_hitColorCoroutine != null)
            StopCoroutine(_hitColorCoroutine);

        // Start the hit color coroutine
        _hitColorCoroutine = StartCoroutine(ShieldColorFade(hitColor));
    }

    private IEnumerator ShieldColorFade(Color targetColor)
    {
        // Get the start color
        var startColor = _initialColor;

        var startTime = Time.time;
        var duration = hitColorCurve[hitColorCurve.keys.Length - 1].time;

        while (Time.time < startTime + duration)
        {
            // Calculate the current color
            var currentTime = Time.time - startTime;
            var t = hitColorCurve.Evaluate(currentTime);

            var currentColor = Color.Lerp(startColor, targetColor, t);

            // Set the color property
            ForceColor(currentColor);

            yield return null;
        }

        // Set the color property
        ForceColor(_initialColor);
    }

    private void ForceColor(Color targetColor)
    {
        // Force the color property for each material
        foreach (var shieldRenderer in shieldRenderers)
            shieldRenderer.sharedMaterial.SetColor(ColorPropertyID, targetColor);
    }

    private void ForceFade(float targetValue)
    {
        foreach (var shieldRenderer in shieldRenderers)
            shieldRenderer.sharedMaterial.SetFloat(FadePropertyID, targetValue);
    }

    private void ForceShieldCollider(bool isActive)
    {
        shieldCollider.enabled = isActive;
    }
}