using System;
using UnityEngine;

public class MultiColliderHelper : MonoBehaviour
{
    [SerializeField] private bool collidersEnabledOnAwake = true;
    [SerializeField] private Collider[] colliders;

    private void Awake()
    {
        if (collidersEnabledOnAwake)
            EnableColliders();
        else
            DisableColliders();
    }

    private static void SetColliders(Collider[] colliders, bool isOn)
    {
        foreach (var collider in colliders)
            collider.enabled = isOn;
    }

    public void EnableColliders() => SetColliders(colliders, true);
    
    public void DisableColliders() => SetColliders(colliders, false);
}