using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GauntletInspect : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        // Get the Animator component
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Check for "I" key press
        if (Input.GetKeyDown(KeyCode.I))
        {
            // Trigger the animation
            animator.SetTrigger("PlayTrigger");
        }
    }
}
