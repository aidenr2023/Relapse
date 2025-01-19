using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableParentOnTrigger : MonoBehaviour
{
    [Tooltip("Drag the parent GameObject that should be disabled/enabled here.")]
    public GameObject parentObject;

    [Tooltip("The tag used to identify the player object.")]
    public string playerTag = "Player";

    // Called when something enters this trigger collider
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            // Disable the parent object
            parentObject.SetActive(false);
        }
    }

    // Called when something exits this trigger collider
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            // Re-enable the parent object
            parentObject.SetActive(true);
        }
    }
}
