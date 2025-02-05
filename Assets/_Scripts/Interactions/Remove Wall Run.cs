using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RemoveWallRun : MonoBehaviour
{
    public GameObject targetObject;
    public string newLayer = "Default";
    public Rigidbody playerRigidbody;
    public float freezeDuration = 2f;
    public GameObject objectToDisable;
    public float disableDelay = 3f;

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag =="Player")
        {
               if (targetObject !=null)
               {
                targetObject.layer = LayerMask.NameToLayer(newLayer);
               } 
        }

        if (playerRigidbody != null)
        {
            StartCoroutine(FreezePlayerXZ());
        }
        if (objectToDisable != null)
        {
            StartCoroutine(DisableObject());
        }
    }

    private IEnumerator FreezePlayerXZ()
    {
        RigidbodyConstraints originalConstraints = playerRigidbody.constraints;
        playerRigidbody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
        
        yield return new WaitForSeconds(freezeDuration);
        playerRigidbody.constraints = originalConstraints;
    }
    private IEnumerator DisableObject()
    {
        yield return new WaitForSeconds(disableDelay);
        objectToDisable.SetActive(false);
    }
}
