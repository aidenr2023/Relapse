using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using UnityEngine;

public class VoidPhase1 : MonoBehaviour
{
    [SerializeField] private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
        animator.Play("VoidPhase0");
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Triggered by: " + other.gameObject.name);

        if (animator.GetCurrentAnimatorStateInfo(0).IsName("VoidPhase0"))
        {
            Debug.Log("Currently in Void Phase 0 state");
        }
        else if (animator.GetCurrentAnimatorStateInfo(0).IsName("VoidPhase1"))
        {
            Debug.Log("Currently in Void Phase 1 state");
        }

        if (other.gameObject.name == "Collider" && animator.GetCurrentAnimatorStateInfo(0).IsName("VoidPhase0"))
        {
            Debug.Log("Transitioning to Void Phase 1 Animation");
            animator.SetTrigger("Collision1");
        }
        else if (other.gameObject.name == "Collider1" && animator.GetCurrentAnimatorStateInfo(0).IsName("VoidPhase1"))
        {
            Debug.Log("Transitioning to Void Phase 2 Animation");
            animator.SetTrigger("Collision2");
        }
    }
}
