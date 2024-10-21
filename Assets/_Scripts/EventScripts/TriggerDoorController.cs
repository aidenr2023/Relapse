using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerDoorController : MonoBehaviour
{
    [SerializeField] private Animator myDoor = null;
    [SerializeField] private Animator Door2 = null;
    [SerializeField] private bool openTrigger = false;
    [SerializeField] private bool openTrigger2 = false;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            if(openTrigger)
            {
                myDoor.Play("DoorOpen_anim", 0, 0.0f);
            }

            if(openTrigger2)
            {
                
                Door2.Play("DoorOpen2_anim", 0, 0.0f);
            
            }
            Destroy(gameObject);
         }
    }
}
