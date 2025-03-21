using UnityEngine;

public class MindbreakOrbManager : MonoBehaviour
{
    [Header("Checkpoint Settings")]
    [Tooltip("Assign the target checkpoint GameObjects (positions) here.")]
    public GameObject[] checkpointLocations;
    
    [Tooltip("Assign the trigger colliders corresponding to each checkpoint.")]
    public Collider[] checkpointTriggers;
    
    [Header("Movement Settings")]
    [Tooltip("Speed at which the orb moves between checkpoints.")]
    public float moveSpeed = 5f;
    
    [Header("Idle Bobbing Settings")]
    [Tooltip("Amplitude of the up and down bobbing effect when idle.")]
    public float bobAmplitude = 0.5f;
    [Tooltip("Frequency of the bobbing effect.")]
    public float bobFrequency = 1f;

    private Vector3 targetPosition;
    private bool isMoving = false;
    private Vector3 idlePosition; // Base position for bobbing

    // Reference to the player's transform.
    private Transform playerTransform;
    // To track if the player was already inside a given trigger (to fire only on entering)
    private bool[] playerInTrigger;

    private void Start()
    {
        // Initialize the idlePosition to the orb's starting position.
        idlePosition = transform.position;
        targetPosition = idlePosition;

        // Cache the player's transform using the "Player" tag.
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning("Player object not found in scene! Please tag your player as 'Player'.");
        }

        // Initialize the array to track trigger entry state.
        if (checkpointTriggers != null)
            playerInTrigger = new bool[checkpointTriggers.Length];
    }

    private void Update()
    {
        // Only check for trigger entry when not already moving.
        if (!isMoving && playerTransform != null && checkpointTriggers != null)
        {
            for (int i = 0; i < checkpointTriggers.Length; i++)
            {
                // Check if the player's position is inside the trigger's bounds.
                // (Note: This uses the trigger's bounding box, which works for simple shapes.)
                bool isInside = checkpointTriggers[i].bounds.Contains(playerTransform.position);
                // Trigger only on entry (when previously false).
                if (isInside && !playerInTrigger[i])
                {
                    TriggerCheckpoint(i);
                    playerInTrigger[i] = true;
                }
                else if (!isInside)
                {
                    playerInTrigger[i] = false;
                }
            }
        }

        if (isMoving)
        {
            // Move the orb smoothly toward the target position.
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            // Check if the orb has reached (or is very near) the target.
            if (Vector3.Distance(transform.position, targetPosition) < 0.001f)
            {
                transform.position = targetPosition;
                isMoving = false;
                idlePosition = targetPosition;
            }
        }
        else
        {
            // Apply a bobbing (floating) effect when idle.
            float bobOffset = Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
            transform.position = idlePosition + new Vector3(0f, bobOffset, 0f);
        }
    }

    // Call this method to move the orb to the specified checkpoint (by index).
    public void TriggerCheckpoint(int index)
    {
        if (index >= 0 && index < checkpointLocations.Length)
        {
            targetPosition = checkpointLocations[index].transform.position;
            isMoving = true;
        }
        else
        {
            Debug.LogWarning("Checkpoint index is out of range.");
        }
    }
}
