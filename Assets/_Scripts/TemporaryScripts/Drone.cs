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
    public float triggerDelay = 1f; // Delay before triggering events after player enters
    public float downwardDisplacement = 0.5f; // How much the drone moves down when the player lands
    public float recoverySpeed = 2f; // Speed at which the drone returns to original height
    public float pitchDuration = 1.5f; // Duration the pitch stays at 3 before transitioning back
    public float pitchTransitionSpeed = 1f; // Speed of the pitch transition
    public float lightDuration = 2f; // Duration the warning light stays on

    [Header("Interaction Settings")]
    public AudioSource droneAudioSource; // Existing audio source to modify
    public Light warningLight; // Light that turns on temporarily

    private Vector3 originalPosition;
    private Rigidbody rb;
    private bool isTriggered = false;
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
        if (!isTriggered)
        {
            Hover();
            FollowPath();
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
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Only allow landing effects on drones that do not have waypoints assigned
        if (other.CompareTag("Player") && !isTriggered && (waypoints == null || waypoints.Length == 0))
        {
            StartCoroutine(TriggerWithDelay());
        }
    }

    IEnumerator TriggerWithDelay()
    {
        yield return new WaitForSeconds(triggerDelay);

        isTriggered = true;

        if (droneAudioSource != null)
        {
            StartCoroutine(AdjustPitch());
        }

        if (warningLight != null)
        {
            StartCoroutine(EnableLightTemporarily());
        }

        StartCoroutine(DisplaceAndRecover());
    }

    IEnumerator AdjustPitch()
    {
        if (droneAudioSource != null)
        {
            droneAudioSource.pitch = 3f; // Set pitch to 3
            yield return new WaitForSeconds(pitchDuration);

            // Smoothly transition back to 2
            float elapsedTime = 0f;
            while (elapsedTime < pitchTransitionSpeed)
            {
                droneAudioSource.pitch = Mathf.Lerp(3f, 2f, elapsedTime / pitchTransitionSpeed);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            droneAudioSource.pitch = 2f; // Ensure final value is exact
        }
    }

    IEnumerator EnableLightTemporarily()
    {
        if (warningLight != null)
        {
            warningLight.enabled = true;
            yield return new WaitForSeconds(lightDuration);
            warningLight.enabled = false;
        }
    }

    IEnumerator DisplaceAndRecover()
    {
        Vector3 loweredPosition = originalPosition + Vector3.down * downwardDisplacement;

        // Move down smoothly
        while (Vector3.Distance(transform.position, loweredPosition) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, loweredPosition, Time.deltaTime * recoverySpeed);
            yield return null;
        }

        // Move back up smoothly
        while (Vector3.Distance(transform.position, originalPosition) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, originalPosition, Time.deltaTime * recoverySpeed);
            yield return null;
        }

        isTriggered = false;
    }
}
