using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class voidphase2 : MonoBehaviour
{
    // Start is called before the first frame update
    
    [SerializeField] public GameObject Platforms;

    void Start()
    {
       
        Platforms.SetActive(false);
        
    }

    // Update is called once per frame
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            Platforms.SetActive(true);
        }
    }
}
