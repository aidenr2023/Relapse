using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FragmentAppears : MonoBehaviour
{
   public Animator anim;
   public GameObject fragment;
    // Start is called before the first frame update
    void Awake()
    {
        fragment.SetActive(false);
    }

    // Update is called once per frame
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag =="Player")
        {
                fragment.SetActive(true);
               anim.Play("Fragment Movement"); 
        }
    }
}
