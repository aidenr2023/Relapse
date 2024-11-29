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
    private void FindWeapon()
    {
        if (transform.Find("Pistol(clone)"))
        {
            _animator.SetBool("isArmed", true);
        }
        else
        {
            _animator.SetBool("isArmed", false);
        }
    }

    // Check if idle animation should be played
    private void Update()
    {
        if (shouldIdle()) // Add condition for when player is idle, e.g. no movement or actions
        {
            // Start the idle animation if not already idle
            if (!isIdle)
            {
                isIdle = true;
                StartCoroutine(IdleAfterSeconds());
            }
        }
        else
        {
            isIdle = false;
            _animator.SetBool("isIdle", false); // Set isIdle to false immediately if not idle
        }
    }

    // Play idle animation after 10 seconds
    IEnumerator IdleAfterSeconds()
    {
        // Wait for 10 seconds before playing idle animation
        yield return new WaitForSeconds(10);

        // Play idle animation
        _animator.SetBool("isIdle", true);
        
        // Get the length of the idle animation
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
        return Mathf.Abs(GetComponent<Rigidbody>().velocity.magnitude) < 0.1f;
    }
}
    
    

