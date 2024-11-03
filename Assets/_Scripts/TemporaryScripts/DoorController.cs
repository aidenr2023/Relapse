using UnityEngine;
using UnityEngine.UI;

public class DoorController : MonoBehaviour
{
    public Transform lockedSidePosition;    // Set to the position on the locked side of the door
    public Text promptText;                 // UI Text to display messages (e.g., "locked from the other side")
    public float interactionDistance = 2.0f; // Distance from which the player can interact with the door

    private bool isNearDoor = false;        // Is the player near the door
    private Transform player;               // Reference to the player transform
    private bool isLockedFromOtherSide = true; // Initially locked from one side

    private void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        promptText.gameObject.SetActive(false); // Hide prompt text initially
    }

    private void Update()
    {
        if (isNearDoor && Input.GetKeyDown(KeyCode.E))
        {
            float distanceToLockedSide = Vector3.Distance(player.position, lockedSidePosition.position);
            
            if (distanceToLockedSide <= interactionDistance && isLockedFromOtherSide)
            {
                // If near locked side and door is locked, display "locked from the other side" message
                promptText.text = "Locked from the other side";
                promptText.gameObject.SetActive(true);
            }
            else
            {
                // Unlock and open the door
                OpenDoor();
                isLockedFromOtherSide = false; // Unlock the door permanently
                promptText.gameObject.SetActive(false);
            }
        }
        else if (!isNearDoor)
        {
            promptText.gameObject.SetActive(false); // Hide prompt text when not near the door
        }
    }

    private void OpenDoor()
    {
        // Your door opening animation or logic here
        Debug.Log("Door opened!");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isNearDoor = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isNearDoor = false;
        }
    }
}
