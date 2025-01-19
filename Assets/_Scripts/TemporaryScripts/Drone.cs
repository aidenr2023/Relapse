using UnityEngine;
using System.Collections;

public class HoverDrone : MonoBehaviour
{
    [Header("Hover Settings")]
    public float hoverSpeed = 2f; // Speed of the hover bobbing
    public float hoverAmplitude = 0.5f; // Amplitude of the hover bobbing
    public float driftRange = 0.2f; // Range for random horizontal drift
    public float hoverRandomOffset = 0f; // Randomized offset for bobbing

    [Header("Path Settings")]
    public Transform[] waypoints; // Waypoints for the drone to follow
    public float moveSpeed = 2f; // Speed of movement along the path
    public float curveSmoothing = 0.05f; // Smoothing factor for curved movement
    public float waypointThreshold = 0.5f; // Distance to consider a waypoint reached

    [Header("Unstable Behavior Settings")]
    public float destructionDelay = 5f; // Time before the drone explodes
    public float triggerDelay = 1f; // Delay before triggering events after player enters

    [Header("Physics Thruster Settings")]
    public float thrustForce = 15f; // Force of the main thruster
    public Transform thrusterPosition; // Position of the main thruster
    public Transform secondaryThrusterPosition; // Position of the secondary thruster
    public float secondaryThrustForce = 10f; // Force of the secondary thruster

    [Header("Explosion Settings")]
    public ParticleSystem explosionEffect; // Particle system for explosion
    public AudioSource explosionAudioSource; // Audio source for explosion sound

    [Header("Interaction Settings")]
    public AudioSource playerLandingAudioSource; // Audio source for player landing sound
    public Light warningLight; // Light that starts flashing when unstable
    public float lightFlashSpeed = 10f; // Speed of the flashing light

    private Vector3 originalPosition;
    private Rigidbody rb;
    private bool isUnstable = false;
    private int currentWaypointIndex = 0;
    private Vector3 velocity = Vector3.zero; // For smoothing movement

    void Start()
    {
        originalPosition = transform.position;
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; // Initially hovering

        hoverRandomOffset = Random.Range(0f, 2 * Mathf.PI); // Randomize hover offset

        if (warningLight != null)
        {
            warningLight.enabled = false; // Ensure the light is off initially
        }
    }

    void Update()
    {
        if (!isUnstable)
        {
            Hover();
            FollowPath();
        }
        else
        {
            FlashWarningLight();
        }
    }

    void Hover()
    {
        // Bobbing up and down with a randomized phase offset
        float hoverHeight = Mathf.Sin(Time.time * hoverSpeed + hoverRandomOffset) * hoverAmplitude;

        // Random drift
        Vector3 randomDrift = new Vector3(
            Mathf.PerlinNoise(Time.time, 0) * driftRange - driftRange / 2,
            0,
            Mathf.PerlinNoise(0, Time.time) * driftRange - driftRange / 2
        );

        transform.position += new Vector3(0, hoverHeight, 0) + randomDrift * Time.deltaTime;
    }

    void FollowPath()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogWarning("No waypoints assigned to the drone.");
            return;
        }

        Transform targetWaypoint = waypoints[currentWaypointIndex];

        // Calculate smoothed movement
        Vector3 targetPosition = Vector3.Lerp(transform.position, targetWaypoint.position, curveSmoothing);
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, 1f / moveSpeed);

        // Rotate to face the movement direction
        Vector3 direction = (targetPosition - transform.position).normalized;
        if (direction.magnitude > 0.01f) // Avoid jittering when stationary
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * moveSpeed);
        }

        // Check if the waypoint is reached
        float distance = Vector3.Distance(transform.position, targetWaypoint.position);
        if (distance < waypointThreshold)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            Debug.Log($"Reached waypoint {currentWaypointIndex}, moving to next waypoint.");
        }

        // Debug line in the Scene view to visualize the waypoint path
        Debug.DrawLine(transform.position, targetWaypoint.position, Color.green);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isUnstable)
        {
            StartCoroutine(TriggerWithDelay());
        }
    }

    IEnumerator TriggerWithDelay()
    {
        yield return new WaitForSeconds(triggerDelay);

        Debug.Log("Player landed on the drone!");
        isUnstable = true;
        rb.isKinematic = false; // Enable physics

        if (playerLandingAudioSource != null)
        {
            playerLandingAudioSource.Play();
        }

        if (warningLight != null)
        {
            warningLight.enabled = true; // Start flashing the light
        }

        StartCoroutine(ApplyThrusters());
        Invoke("Explode", destructionDelay); // Schedule explosion
    }

    IEnumerator ApplyThrusters()
    {
        while (isUnstable)
        {
            if (rb != null && thrusterPosition != null)
            {
                // Apply force from the main thruster
                Vector3 localUp = thrusterPosition.up;
                rb.AddForceAtPosition(localUp * thrustForce, thrusterPosition.position, ForceMode.Force);
            }

            if (rb != null && secondaryThrusterPosition != null)
            {
                // Apply force from the secondary thruster to add rotational instability
                Vector3 localUp = secondaryThrusterPosition.up;
                rb.AddForceAtPosition(localUp * secondaryThrustForce, secondaryThrusterPosition.position, ForceMode.Force);
            }

            yield return null;
        }
    }

    void FlashWarningLight()
    {
        if (warningLight != null)
        {
            float intensity = Mathf.Abs(Mathf.Sin(Time.time * lightFlashSpeed));
            warningLight.intensity = intensity;
        }
    }

    void Explode()
    {
        if (explosionEffect != null)
        {
            ParticleSystem instantiatedEffect = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            instantiatedEffect.Play();
            Destroy(instantiatedEffect.gameObject, instantiatedEffect.main.duration);
        }

        if (explosionAudioSource != null)
        {
            AudioSource instantiatedAudioSource = Instantiate(explosionAudioSource, transform.position, Quaternion.identity);
            instantiatedAudioSource.Play();
            Destroy(instantiatedAudioSource.gameObject, instantiatedAudioSource.clip.length);
        }

        Destroy(gameObject); // Destroy the drone
    }
}
