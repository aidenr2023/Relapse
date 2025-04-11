using UnityEngine;
using System.Collections;

public class HoverDrone : MonoBehaviour
{
    [Header("Hover Settings")] public float hoverSpeed = 2f; // Speed of the hover bobbing
    public float hoverAmplitude = 0.5f; // Amplitude of the hover bobbing
    public float driftRange = 0.2f; // Range for random horizontal drift
    public float hoverRandomOffset = 0f; // Randomized offset for bobbing

    [Header("Path Settings")] public Transform[] waypoints; // Waypoints for the drone to follow
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

    [Header("Interaction Settings")] public AudioSource droneAudioSource; // Existing audio source to modify
    public Light warningLight; // Light that turns on temporarily

    private Vector3 originalPosition;
    private Vector3 basePosition; // Base position used for path following
    private Rigidbody rb;
    private bool isTriggered = false;
    private int currentWaypointIndex = 0;
    private Vector3 velocity = Vector3.zero; // For smoothing movement

    private void Start()
    {
        originalPosition = transform.position;
        basePosition = transform.position;
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; // Initially hovering

        hoverRandomOffset = Random.Range(0f, 2 * Mathf.PI); // Randomize hover offset

        if (warningLight != null)
        {
            warningLight.enabled = false; // Ensure the light is off initially
        }
    }

    private void Update()
    {
        if (isTriggered) 
            return;
        
        // Update the base position from the path (or keep it at originalPosition)
        if (waypoints != null && waypoints.Length > 0)
            FollowPath();

        else
            basePosition = originalPosition;

        // Apply the hover offset on top of the base position
        ApplyHoverOffset();
    }

    private void ApplyHoverOffset()
    {
        // Vertical bobbing
        var verticalOffset = Mathf.Sin(Time.time * hoverSpeed + hoverRandomOffset) * hoverAmplitude;
        // Horizontal drift that oscillates (centered around 0)
        var driftOffset = new Vector3(
            (Mathf.PerlinNoise(Time.time, 0) - 0.5f) * driftRange,
            0,
            (Mathf.PerlinNoise(0, Time.time) - 0.5f) * driftRange
        );
        transform.position = basePosition + new Vector3(0, verticalOffset, 0) + driftOffset;
    }

    private void FollowPath()
    {
        if (waypoints == null || waypoints is null || waypoints.Length == 0)
            return;

        // Set the target waypoint
        currentWaypointIndex %= waypoints.Length;
        var targetWaypoint = waypoints[currentWaypointIndex];

        // Smoothly update the base position toward the target waypoint
        var targetBasePos = Vector3.Lerp(basePosition, targetWaypoint.position, curveSmoothing);
        basePosition = Vector3.SmoothDamp(basePosition, targetBasePos, ref velocity, 1f / moveSpeed);

        // Rotate to face the movement direction (using the base position for consistency)
        var direction = (targetBasePos - basePosition).normalized;
        if (direction.magnitude > 0.01f)
        {
            var targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * moveSpeed);
        }

        // Check if the waypoint is reached (using basePosition to avoid hover-induced false positives)
        var distance = Vector3.Distance(basePosition, targetWaypoint.position);
        if (distance < waypointThreshold)
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only allow landing effects on drones that do not have waypoints assigned
        if (other.CompareTag("Player") && !isTriggered && (waypoints == null || waypoints.Length == 0))
            StartCoroutine(TriggerWithDelay());
    }

    private IEnumerator TriggerWithDelay()
    {
        yield return new WaitForSeconds(triggerDelay);

        isTriggered = true;

        if (droneAudioSource != null)
            StartCoroutine(AdjustPitch());

        if (warningLight != null)
            StartCoroutine(EnableLightTemporarily());

        StartCoroutine(DisplaceAndRecover());
    }

    private IEnumerator AdjustPitch()
    {
        if (droneAudioSource != null)
        {
            droneAudioSource.pitch = 3f; // Set pitch to 3
            yield return new WaitForSeconds(pitchDuration);

            // Smoothly transition back to 2
            var elapsedTime = 0f;
            while (elapsedTime < pitchTransitionSpeed)
            {
                droneAudioSource.pitch = Mathf.Lerp(3f, 2f, elapsedTime / pitchTransitionSpeed);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            droneAudioSource.pitch = 2f; // Ensure final value is exact
        }
    }

    private IEnumerator EnableLightTemporarily()
    {
        if (warningLight != null)
        {
            warningLight.enabled = true;
            yield return new WaitForSeconds(lightDuration);
            warningLight.enabled = false;
        }
    }

    private IEnumerator DisplaceAndRecover()
    {
        var loweredPosition = originalPosition + Vector3.down * downwardDisplacement;

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