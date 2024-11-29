using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MoveOnTrigger : MonoBehaviour
{
    public GameObject targetObject; // The object to move
    public float speed = 5f;        // Speed of movement
    public float duration = 2f;     // Duration to move for
     private bool isMoving = false;  // Is the object currently moving
    [SerializeField] private Animator animator; // Animator component for the object
    // sound of the object
    [SerializeField] private AudioSource audioSource; // ManagedAudioSource component for the object

    //reference to NPCmovement script
    public EnemyAnimatorController npcMovement;
    
    //on awake get the NPCmovement script
     // private void Awake()
     // {
     //     npcMovement = GetComponent<NPCmovement>();
     // }
    

    private void OnTriggerEnter(Collider other)
    {
            audioSource.PlayOneShot(audioSource.clip);
            //npcMovement.EnableMovement();

          //  StartCoroutine(MoveObject());
    }

    // private IEnumerator MoveObject()
    // {
    //  // is moving is true
    //  isMoving = true;
    //  //get the NPCmovement script and disable movement
    //  npcMovement.DisableMovement();
    //  
    //  //play animation when speed is 1 
    //  animator.SetFloat("Speed", 1);
    //
    //  // Store the initial position of the target object
    //  Vector3 startPosition = targetObject.transform.position;
    //  Vector3 endPosition = startPosition + (transform.forward * speed * duration);
    //
    //  float elapsedTime = 0f;
    //
    //  while (elapsedTime < duration)
    //  {
    //   // Move the object forward
    //   targetObject.transform.position = Vector3.Lerp(startPosition, endPosition, (elapsedTime / duration));
    //   elapsedTime += Time.deltaTime;
    //   yield return null; // Wait for the next frame
    //  }

     // // Ensure the object ends up exactly at the end position
     // targetObject.transform.position = endPosition;
     //
     // isMoving = false;
     //
     //get the NPCmovement script and enable movement
     
    }

    // Destroy the trigger volume after movement is completed
         //This destroys the GameObject this script is attached to
         //Destroy(gameObject);
        //stop animation when speed is 0
        //animator.SetFloat("Speed",0);
        //is moving is false
        //isMoving = false;
        
    

