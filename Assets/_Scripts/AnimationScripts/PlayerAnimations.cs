using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimations : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    private bool isIdle = false;
    
    // private void Start()
    // {
    //     _animator = GetComponent<Animator>();
    // }

    // Find and set whether the player is armed
    // private void FindWeapon()
    // {
    //     if (transform.Find("Pistol") != null)
    //     {
    //         _animator.SetBool("isArmed", true);
    //     }
    //     else
    //     {
    //         _animator.SetBool("isArmed", false);
    //     }
    // }

    // Check if idle animation should be played
    private void Update()
    {
       
    }

    // Play idle animation after 10 seconds
    IEnumerator IdleAfterSeconds()
    {
        // Wait for 5 seconds before playing idle animation
        yield return new WaitForSeconds(5);

        // Play idle animation
        _animator.SetBool("isIdle", true);
        
        // Get the length of the  second  idle animation
        float animationTime = _animator.GetCurrentAnimatorStateInfo(0).length;

        // Wait for the length of the idle animation
        yield return new WaitForSeconds(animationTime);

        // Reset isIdle after the idle animation finishes
        _animator.SetBool("isIdle", false);
    }

    // Condition to check if the player should be considered idle
    private bool shouldIdle()
    {
        // Example: Check if the player is not moving or performing actions
        // debug log to check if player is idle
        Debug.Log("Player is idle: " + (Mathf.Abs(GetComponent<Rigidbody>().velocity.magnitude) < 0.1f));
        return Mathf.Abs(GetComponent<Rigidbody>().velocity.magnitude) < 0.1f;
    }
}
    
    

