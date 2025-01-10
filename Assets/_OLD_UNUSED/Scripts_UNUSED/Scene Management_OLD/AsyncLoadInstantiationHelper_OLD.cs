using System;
using UnityEngine;

public class AsyncLoadInstantiationHelper : MonoBehaviour
{
    [SerializeField] private InstantiationInformation[] instantiateOnAwake;
    [SerializeField] private InstantiationInformation[] instantiateOnStart;

    private void Awake()
    {
        // Instantiate the prefabs on awake
        foreach (var info in instantiateOnAwake)
            info.InstantiatePrefab();
    }

    private void Start()
    {
        // Instantiate the prefabs on start
        foreach (var info in instantiateOnStart)
            info.InstantiatePrefab();
    }

    [Serializable]
    private class InstantiationInformation
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private Transform positionRotation;

        public void InstantiatePrefab()
        {
            Debug.Log($"Instantiating {prefab.name} at {positionRotation.position} with rotation {positionRotation.rotation}");

            // Instantiate the prefab at the position and rotation
            Instantiate(prefab, positionRotation.position, positionRotation.rotation);
        }
    }
}