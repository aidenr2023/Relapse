using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveOnTrigger : MonoBehaviour
{
    public GameObject targetObject; // The object to move
    public float speed = 5f;        // Speed of movement
    public float duration = 2f;     // Duration to move for

    private bool isMoving = false;  // Is the object currently moving

    private void OnTriggerEnter(Collider other)
    {
        // Start moving the target object when the trigger is entered
        if (!isMoving)
        {
            StartCoroutine(MoveObject());
        }
    }

    private IEnumerator MoveObject()
    {
        isMoving = true;

        // Store the initial position of the target object
        Vector3 startPosition = targetObject.transform.position;
        Vector3 endPosition = startPosition + (transform.forward * speed * duration);

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // Move the object forward
            targetObject.transform.position = Vector3.Lerp(startPosition, endPosition, (elapsedTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        // Ensure the object ends up exactly at the end position
        targetObject.transform.position = endPosition;

        // Destroy the trigger volume after movement is completed
        Destroy(gameObject); // This destroys the GameObject this script is attached to

        isMoving = false;
    }
}
