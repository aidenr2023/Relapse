using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraneTrigger : MonoBehaviour
{
    public Animation anim;
    // Start is called before the first frame update
    void Awake()
    {
        
    }

    // Update is called once per frame
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag =="Player")
        {
               anim.Play("Rusty Crane"); 
        }
    }
}
