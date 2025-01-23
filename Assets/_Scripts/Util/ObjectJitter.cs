using System;
using UnityEngine;

public class ObjectJitter : MonoBehaviour
{
    [SerializeField] private Vector3 jitterAmount = Vector3.zero;
    [SerializeField, Min(0)] private float jitterSpeed = 1f;

    private float _jitterTimer;

    private void Start()
    {
        // Jitter();
    }

    private void Update()
    {
        // Increment the jitter timer
        _jitterTimer += Time.deltaTime;

        // If the jitter timer is greater than the jitter speed
        if (_jitterTimer < jitterSpeed)
            return;

        // Jitter the object
        Jitter();

        // Reset the jitter timer
        _jitterTimer %= jitterSpeed;
    }

    private void Jitter()
    {
        // Create a random vector3 between -jitterAmount and jitterAmount
        var randomJitter = new Vector3(
            UnityEngine.Random.Range(-jitterAmount.x, jitterAmount.x),
            UnityEngine.Random.Range(-jitterAmount.y, jitterAmount.y),
            UnityEngine.Random.Range(-jitterAmount.z, jitterAmount.z)
        );

        // Set the position of the object towards the random jitter
        transform.localPosition = randomJitter;
    }
}