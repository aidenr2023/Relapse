using UnityEngine;

public class ActivateOnTrigger : MonoBehaviour
{
    // Drag the GameObject you want to activate into this field in the Inspector.
    public GameObject objectToActivate;

    // Ensure the player GameObject has the tag "Player"
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (objectToActivate != null)
            {
                objectToActivate.SetActive(true);
                Debug.Log("Object set to Active");
            }
            else
            {
                Debug.LogWarning("objectToActivate is not assigned in the Inspector.");
            }
        }
    }
}
