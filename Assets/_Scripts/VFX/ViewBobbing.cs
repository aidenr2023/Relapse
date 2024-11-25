using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ViewBobbing : MonoBehaviour
{
    [SerializeField] private CinemachineCameraOffset virtualCam;

    float timer = Mathf.PI / 2;

    [SerializeField] private Vector3 restPosition; //local position of rest when not bobbing
    [SerializeField] private float transitionSpeed = 20f; //smooth transition
    [SerializeField] private float bobSpeed = 4.8f; //how quickly the player's head bobs.
    [SerializeField] private float bobAmount = 0.05f; //how dramatic the bob is.

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        //Make a timer for the bob speed
        timer += bobSpeed *Time.deltaTime;

        //Set the virtual cam offset with some funky trig
        virtualCam.m_Offset = new Vector3(Mathf.Cos(timer) * bobAmount, restPosition.y + Mathf.Abs((Mathf.Sin(timer) * bobAmount)), restPosition.z); //abs val of y for a parabolic path

        //Avoid timer bloat
        if (timer > Mathf.PI * 2) //completed a full cycle on the unit circle. Reset to 0 to avoid bloated values.
            timer = 0;
    }
}
