using UnityEngine;

public class TriggerEnableRigidbody : MonoBehaviour
{
    [Tooltip("The target object that contains (or will have) the Rigidbody component enabled.")]
    public GameObject targetObject;

    // Ensures the trigger only works once.
    private bool hasTriggered = false;

    void OnTriggerEnter(Collider other)
    {
        // Make sure only the player (tagged as "Player") triggers the event.
        if (!hasTriggered && other.CompareTag("Player"))
        {
            hasTriggered = true;
            Rigidbody targetRB = targetObject.GetComponent<Rigidbody>();

            if (targetRB != null)
            {
                // If the Rigidbody exists but was set to isKinematic (i.e., physics disabled),
                // disable kinematic mode to let physics simulation kick in.
                targetRB.isKinematic = false;
                Debug.Log("Player entered! Rigidbody is now active on " + targetObject.name + ".");
            }
            else
            {
                // If there was no Rigidbody attached, add one.
                targetRB = targetObject.AddComponent<Rigidbody>();
                Debug.Log("No Rigidbody found on " + targetObject.name + ", so one was added. Let the physics chaos ensue!");
            }
        }
    }
}
