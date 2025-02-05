using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReverseTrigger : MonoBehaviour
{ 
    //reference to the animator component
    [SerializeField] Animator anim;
   
   //floor object that is shrunk to a invisible state
   [SerializeField] private GameObject floor;
   
   //delay before the floor animation is reversed
   [SerializeField] private float delay = 3f;
   
   static string isTriggered = "isTriggered";
    // Start is called before the first frame update
    void Awake()
    {
    }

    // as the player enters the trigger, the floor will play an animation
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag =="Player")
        {
               anim.SetBool(isTriggered, true);
               //floor.SetActive(false);
        }
    }
    
    //once the player exits the trigger, the floor will play a reverse animation
    private void OnTriggerExit(Collider other)
    {
        if(other.tag =="Player")
        {
            StartCoroutine(Reverse());
        }
    }
    
    //turn off the floor object function (optional)
    public void Turnoff()
    {
        floor.SetActive(false);
    }

    //turn on the floor object function (optional)
    public void Turnon()
    {
        floor.SetActive(true);
    }
    
    // Reverse the animation
    IEnumerator Reverse()
    {
        yield return new WaitForSeconds(delay);
        //floor.SetActive(true);
        anim.SetBool(isTriggered, false);
        
    }
}
