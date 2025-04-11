using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PostProcessingVolumeController : MonoBehaviour
{
    public static PostProcessingVolumeController Instance { get; private set; }

    #region Serialized Fields

    [SerializeField] private PostProcessingType currentType = PostProcessingType.Apartment;

    [Space, SerializeField] private DynamicPostProcessPair apartmentPair;
    [SerializeField] private DynamicPostProcessPair cityPair;
    [SerializeField] private DynamicPostProcessPair cityCombatPair;
    [SerializeField] private DynamicPostProcessPair mindbreakPair;

    #endregion

    private DynamicPostProcessPair _currentPair;

    private readonly Dictionary<DynamicPostProcessPair, Coroutine> _coroutines = new();

    #region Getters

    public DynamicPostProcessVolume ScreenVolume => _currentPair.ScreenVolume;
    public DynamicPostProcessVolume WorldVolume => _currentPair.WorldVolume;
    public DynamicVignetteModule VignetteModule => ScreenVolume.VignetteModule;

    private DynamicPostProcessPair[] Pairs => new[]
    {
        apartmentPair, cityPair, cityCombatPair, mindbreakPair,
    };

    #endregion

    private void Awake()
    {
        // If there is already an instance, log an error!
        if (Instance != null)
        {
            Debug.LogError("There is already an instance of PostProcessingVolumeController in the scene!");
            Destroy(gameObject);
            return;
        }

        // Set the instance to this
        Instance = this;

        // Force the correct post-processing on start
        ChangePostProcessing(currentType, 0);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void ChangePostProcessing(PostProcessingType type, float duration = .5f)
    {
        // Debug.Log($"Changing Post Processing to {type}");

        // Set the current type to the new type
        currentType = type;

        // Keep track of the old pair
        var oldPair = _currentPair;

        // Set the current pair to the new pair
        _currentPair = type switch
        {
            PostProcessingType.None => _currentPair,
            PostProcessingType.Apartment => apartmentPair,
            PostProcessingType.City => cityPair,
            PostProcessingType.Mindbreak => mindbreakPair,
            // PostProcessingType.CityCombat => cityCombatPair,
            PostProcessingType.CityCombat => cityPair,
            _ => throw new ArgumentOutOfRangeException()
        };

        // Create an array containing all the pairs
        var allPairs = Pairs;

        foreach (var pair in allPairs)
        {
            // Continue if the pair is the current pair
            if (pair == _currentPair)
                continue;

            // Stop the current coroutine if it exists
            if (_coroutines.TryGetValue(pair, out var cCoroutine))
            {
                if (cCoroutine != null)
                    StopCoroutine(cCoroutine);

                _coroutines.Remove(pair);
            }

            // Start the coroutine to fade the weight of the pair OUT
            _coroutines[pair] = StartCoroutine(SetVolumeWeight(pair, 0, duration));
        }

        // Stop the current coroutine if it exists
        if (_coroutines.TryGetValue(_currentPair, out var currentRoutine))
        {
            if (currentRoutine != null)
                StopCoroutine(currentRoutine);

            _coroutines.Remove(_currentPair);
        }

        // Start the coroutine to fade the weight of the current pair IN
        _coroutines[_currentPair] = StartCoroutine(SetVolumeWeight(_currentPair, 1, duration));

        // Transfer the tokens from the old pair to the new pair
        if (oldPair != null)
        {
            oldPair.ScreenVolume.TransferTokens(_currentPair.ScreenVolume);
            oldPair.WorldVolume.TransferTokens(_currentPair.WorldVolume);
        }
    }

    private static IEnumerator SetVolumeWeight(DynamicPostProcessPair pair, float targetWeight, float duration)
    {
        // If the transition is instant
        if (duration <= 0)
        {
            pair.SetWeight(targetWeight);
            yield break;
        }

        // Get the current weight
        var currentWeight = pair.Weight;

        // Get the start time
        var startTime = Time.time;

        // Loop until the duration is reached
        while (Time.time < startTime + duration)
        {
            // Calculate the new weight
            var newWeight = Mathf.Lerp(currentWeight, targetWeight, (Time.time - startTime) / duration);

            // Set the weight
            pair.SetWeight(newWeight);

            // Wait for the next frame
            yield return null;
        }

        // Set the weight to the target weight
        pair.SetWeight(targetWeight);
    }
}