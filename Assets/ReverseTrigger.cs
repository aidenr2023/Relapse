using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReverseTrigger : MonoBehaviour
{
    public Animator anim;
   public GameObject floor;
   public float delay = 3f;
    // Start is called before the first frame update
    void Awake()
    {
    }

    // Update is called once per frame
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag =="Player")
        {
               floor.SetActive(true);
               anim.Play("Floor Shrink", -1, -1f);
        }
    }
}
