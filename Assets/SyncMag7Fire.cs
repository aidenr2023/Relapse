using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncMag7Fire : MonoBehaviour
{
    
    public void PlayTriggerOnShootString(string objectName)
    {
        // Use something like:
        GameObject obj = GameObject.Find(objectName);
        if (obj)
        {
            Animator anim = obj.GetComponent<Animator>();
            anim?.SetTrigger("Cocking");
        }
    }

}

