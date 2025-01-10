// https://discussions.unity.com/t/automatically-assigning-gameobjects-a-unique-and-consistent-id-any-ideas/75104/3

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Placeholder for UniqueIdDrawer script
public class UniqueIdentifierAttribute : PropertyAttribute
{
}

[ExecuteInEditMode]
public class UniqueId : MonoBehaviour
{
    #if UNITY_EDITOR

    // A static dictionary to store the unique IDs
    private static readonly Dictionary<string, UniqueId> UniqueIdDictionary = new();

    #endif

    [SerializeField, UniqueIdentifier] private string uniqueId;
    [SerializeField, Readonly] private string originalScene;

    public string UniqueIdValue => uniqueId;

    public string OriginalScene => originalScene;

    private void Awake()
    {
        GenerateNewUniqueIdIfNeeded();
        originalScene = gameObject.scene.name;
    }

    private void OnDisable()
    {
        GenerateNewUniqueIdIfNeeded();
        SetOriginalSceneIfNeeded();
    }

    private void Update()
    {
        GenerateNewUniqueIdIfNeeded();
        SetOriginalSceneIfNeeded();
    }

    private void GenerateNewUniqueIdIfNeeded()
    {
#if UNITY_EDITOR

        // if in the editor
        if (!Application.isPlaying)
        {
            // If this object's unique id is not in the dictionary, add it
            if (!UniqueIdDictionary.ContainsKey(uniqueId))
                UniqueIdDictionary.Add(uniqueId, this);

            // If the dictionary DOES contain the unique ID, but the UniqueID object does not match, then this object was copied
            // Regenerate the unique ID
            if (UniqueIdDictionary.ContainsKey(uniqueId) && UniqueIdDictionary[uniqueId] != this)
            {
                uniqueId = Guid.NewGuid().ToString();
                Debug.Log($"Generated new Unique ID for {gameObject.name} to {uniqueId}!");
            }

            // prevent any actual code from running
            return;
        }

#endif
    }

    private void SetOriginalSceneIfNeeded()
    {
#if UNITY_EDITOR

        if (!Application.isPlaying)
            originalScene = gameObject.scene.name;
#endif
    }
}