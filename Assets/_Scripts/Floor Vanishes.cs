using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorVanishes : MonoBehaviour
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
               anim.Play("Floor Shrink");
               StartCoroutine(Shrink()); 
        }
    }
    private IEnumerator Shrink()
    {
        yield return new WaitForSeconds(delay);
        floor.SetActive(false); 
    }
}
