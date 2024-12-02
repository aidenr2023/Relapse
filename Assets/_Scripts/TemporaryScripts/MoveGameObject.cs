using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveGameObject : MonoBehaviour
{
    public GameObject targetObject; // The object to move
    public Vector3 targetPosition; // The world-space coordinates to move the object to
    public float moveDuration = 2f; // The time (in seconds) it takes to move the object

    private bool hasTriggered = false; // Ensures the action happens only once
    private bool isMoving = false; // Tracks if the object is currently moving
    private Vector3 startPosition; // The object's starting position
    private float elapsedTime = 0f; // Tracks how much time has passed during the movement
    private Collider[] colliders; // Array to store all colliders on the target object and its children

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered || targetObject == null) return;

        if (other.CompareTag("Player"))
        {
            hasTriggered = true; // Mark as triggered
            isMoving = true; // Start moving the object
            startPosition = targetObject.transform.position; // Record the starting position
            elapsedTime = 0f; // Reset the elapsed time

            // Disable collisions for the target object and its children
            colliders = targetObject.GetComponentsInChildren<Collider>();
            foreach (var col in colliders)
            {
                col.enabled = false;
            }
        }
    }

    private void Update()
    {
        if (isMoving)
        {
            elapsedTime += Time.deltaTime;

            // Calculate the proportion of time passed
            float t = Mathf.Clamp01(elapsedTime / moveDuration);

            // Interpolate position
            targetObject.transform.position = Vector3.Lerp(startPosition, targetPosition, t);

            // Stop moving after the specified duration
            if (t >= 1f)
            {
                isMoving = false;

                // Re-enable collisions for the target object and its children
                foreach (var col in colliders)
                {
                    col.enabled = true;
                }
            }
        }
    }
}