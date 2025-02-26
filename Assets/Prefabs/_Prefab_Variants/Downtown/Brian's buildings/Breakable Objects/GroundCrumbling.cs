using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class GroundCrumbling : MonoBehaviour
{
    //reference to the timeline component
    [SerializeField] public PlayableDirector crumble;
   
   //floor object that is shrunk to a invisible state
   [SerializeField] private GameObject crumblingfloor;
   
   [SerializeField] private GameObject originalfloor;
   //delay before the floor animation is reversed
   [SerializeField] private float delay = 3f;
   
   static string isTriggered = "isTriggered";
    // Start is called before the first frame update
    void Awake()
    {
        Turnoff();
    }

    // as the player enters the trigger, the floor will play an animation
    public void OnTriggerEnter(Collider other)
    {
        if(other.tag =="Player")
        {
            Play();
            Turnon();
            //floor.SetActive(false);
        }
    }

    public void Play()
    {
        crumble.Play();
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
        crumblingfloor.SetActive(false);
        originalfloor.SetActive(true);
    }

    //turn on the floor object function (optional)
    public void Turnon()
    {
        crumblingfloor.SetActive(true);
        originalfloor.SetActive(false);
    }
    
    // Reverse the animation
    IEnumerator Reverse()
    {
        yield return new WaitForSeconds(delay);
        //floor.SetActive(true);
        
        
    }
}
