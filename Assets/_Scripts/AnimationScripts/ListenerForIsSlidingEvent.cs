using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ListenerForIsSlidingEvent : MonoBehaviour
{
    //reference to slide script
    private PlayerSlide _playerSlide;
    
    // Start is called before the first frame update
    void Start()
    {
        //get the slide script
        //_playerSlide = GetComponent<PlayerSlide>();
        
        //get the slide script from the parent object
        _playerSlide = GetComponentInParent<PlayerSlide>();

    }
    //add the event listener
    // private void OnEnable()
    // {
    //    // _playerSlide.OnSlideStart += HandleIsSliding();
    // }
    // //remove the event listener
    // private void OnDisable()
    // {
    //     _playerSlide.OnSlideEnd -= HandleIsSliding();
    // }
    //
    
    // //method to handle the event
    // public void TurnOffGameobject()
    // {
    //     //turn off the gameobject
    //     _playerSlide.gameObject.SetActive(false);
    // }
}
