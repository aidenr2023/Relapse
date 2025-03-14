using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class CustomTrigger : MonoBehaviour
{
    [SerializeField, Min(0)] private float enterActivationDelay = 0;
    [SerializeField, Min(0)] private float exitActivationDelay = 0;

    [SerializeField] private bool activateEnterOnce = false;
    [SerializeField] private bool activateExitOnce = false;
    
    [SerializeField] private UnityEvent onTriggerEnter;
    [SerializeField] private UnityEvent onTriggerExit;

    private bool _hasEntered;
    private bool _hasExited;
    
    private void OnTriggerEnter(Collider other)
    {
        // Return if the other object is not the player
        if (!other.CompareTag("Player"))
            return;

        // Return if the trigger should only activate once and has already been entered
        if (activateEnterOnce && _hasEntered)
            return;
        
        // Set the has entered flag
        _hasEntered = true;
        
        StartCoroutine(DelayedActivation(onTriggerEnter, enterActivationDelay));
    }

    private void OnTriggerExit(Collider other)
    {
        // Return if the other object is not the player
        if (!other.CompareTag("Player"))
            return;
        
        if (activateExitOnce && _hasExited)
            return;
        
        // Set the has exited flag
        _hasExited = true;
        
        StartCoroutine(DelayedActivation(onTriggerExit, exitActivationDelay));
    }

    private static IEnumerator DelayedActivation(UnityEvent unityEvent, float delay)
    {
        // Wait for the delay
        yield return new WaitForSeconds(delay);
        
        // Invoke the event
        unityEvent.Invoke();
    }
}