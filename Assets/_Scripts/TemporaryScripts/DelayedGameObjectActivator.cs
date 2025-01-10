using System;
using UnityEngine;

public class DelayedGameObjectActivator : MonoBehaviour
{
    [SerializeField] private GameObject targetGameObject;
    [SerializeField] private float delay = 1f;

    private bool _activated;

    private void ActivateGameObject()
    {
        // if already activated, return
        if (_activated)
            return;

        targetGameObject.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        // if already activated, return
        if (_activated)
            return;

        Invoke(nameof(ActivateGameObject), delay);
    }
}