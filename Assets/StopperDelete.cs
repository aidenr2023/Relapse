using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StopperDelete : MonoBehaviour
{
    [SerializeField]  GameObject Stopper;

    void Start()
    {
        
    }

    // Update is called once per frame
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            StartCoroutine(Delete());
        }
    }

    private IEnumerator Delete ()
    {
        yield return new WaitForSeconds(2f);
        Stopper.SetActive(false);
    }
}
