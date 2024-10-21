using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorKeyScript : MonoBehaviour
{
    public GameObject elevatorShaft;
    [SerializeField] private Animator HatchDoor = null;

    // Start is called before the first frame update
    void Start()
    {
        if(elevatorShaft != null)
        {
            elevatorShaft.SetActive(true); // Ensure elevator shaft starts inactive
        }
        else
        {
            Debug.LogWarning("Elevator shaft is not assigned.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (elevatorShaft != null)
            {
                elevatorShaft.SetActive(true); // Activate the elevator shaft
                HatchDoor.Play("Hatch_anim", 0, 0.0f);
            }

            Destroy(gameObject); // Destroy the key object
        }
    }
}
