// https://discussions.unity.com/t/automatically-assigning-gameobjects-a-unique-and-consistent-id-any-ideas/75104/3

using System;
using UnityEngine;
using System.Collections;

// Placeholder for UniqueIdDrawer script
public class UniqueIdentifierAttribute : PropertyAttribute
{
}

[ExecuteInEditMode]
public class UniqueId : MonoBehaviour
{
    [SerializeField, UniqueIdentifier] private string uniqueId;
    [SerializeField, Readonly] private int instanceID;

    public string UniqueIdValue => uniqueId;

    private void Awake()
    {
        GenerateNewInstanceIdIfNeeded();
    }

    private void OnDisable()
    {
        GenerateNewInstanceIdIfNeeded();
    }

    private void Update()
    {
        GenerateNewInstanceIdIfNeeded();
    }

    private void GenerateNewInstanceIdIfNeeded()
    {
#if UNITY_EDITOR

        // if in the editor
        if (!Application.isPlaying)
        {
            // if the instance ID doesn't match then this was copied!
            if (instanceID != gameObject.GetInstanceID())
            {
                instanceID = gameObject.GetInstanceID();

                // Regenerate the unique ID
                uniqueId = Guid.NewGuid().ToString();

                Debug.Log($"Regenerated Unique ID for {gameObject.name} to {uniqueId}!");
            }

            // this object wasn't copied but set its ID to check for further copies
            else if (instanceID == 0)
                instanceID = gameObject.GetInstanceID();

            // prevent any actual code from running
            return;
        }

#endif
    }
}