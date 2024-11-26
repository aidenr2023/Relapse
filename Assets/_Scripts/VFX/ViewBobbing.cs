using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;

public class ViewBobbing : MonoBehaviour
{
    [SerializeField] private CinemachineCameraOffset virtualCam; //reference to the virtual cam
    [SerializeField] private Vector3 restPosition; //local position of rest when not bobbing
    [SerializeField] private float transitionSpeed = 20f; //smooth transition
    [SerializeField] private float bobSpeed = 4.8f; //how quickly the player's head bobs.
    [SerializeField] private float bobAmount = 0.05f; //how dramatic the bob is.

    float timer = Mathf.PI / 2; //When sin is 1
    Vector3 camPos;

    void Awake()
    {
        camPos = transform.localPosition;
    }

    void Update()
    {
        
        if (true) //*Add an if to check if player is moving*
        {
            //Make a timer for the bob speed
            timer += bobSpeed * Time.deltaTime;

            //Set the virtual cam offset with some funky trig
            virtualCam.m_Offset = new Vector3(Mathf.Cos(timer) * bobAmount, restPosition.y + Mathf.Abs((Mathf.Sin(timer) * bobAmount)), restPosition.z); //abs val of y for a parabolic path
        }
        else
        {
            timer = Mathf.PI / 2; //reinitialize
            virtualCam.m_Offset = new Vector3(Mathf.Lerp(camPos.x, restPosition.x, transitionSpeed * Time.deltaTime), Mathf.Lerp(camPos.y, restPosition.y, transitionSpeed * Time.deltaTime), Mathf.Lerp(camPos.z, restPosition.z, transitionSpeed * Time.deltaTime)); //transition smoothly from walking to stopping.
            camPos = virtualCam.m_Offset; 
        }

        //Avoid timer bloat
        if (timer > Mathf.PI * 2) //completed a full cycle on the unit circle. Reset to 0 to avoid bloated values.
            timer = 0;
    }
}