using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAnimatorController : MonoBehaviour
{
    // Reference to the NavMeshAgent
    public NavMeshAgent navMeshAgent;

    // Reference to the Animator
    public Animator animator;

    // public bool canMove = false;

    void Update()
    {
        // return;

        // Get the current speed from the NavMeshAgent
        float speed = navMeshAgent.velocity.magnitude;

        // Set the Speed parameter in the Animator
        animator.SetFloat("Speed", speed);

        // Optionally, you can control other NPC behaviors or animations here as well
        animator.speed = navMeshAgent.speed / 3f;
    }

    // public void DisableMovement()
    // {
    //     canMove = false;
    //     navMeshAgent.enabled = false;
    //     //navMeshAgent.isStopped = true;
    //    // animator.SetFloat("Speed", 0);
    // }
    //
    // public void EnableMovement()
    // {
    //     canMove = true;
    //     navMeshAgent.enabled = true;
    //     //navMeshAgent.isStopped = false;
    //     animator.SetFloat("Speed", 2);
    // }
}