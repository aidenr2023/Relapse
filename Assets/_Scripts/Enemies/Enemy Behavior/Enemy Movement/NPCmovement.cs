using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCmovement : MonoBehaviour
{
    public UnityEngine.AI.NavMeshAgent navMeshAgent;  // Reference to the NavMeshAgent
    public Animator animator;          // Reference to the Animator
    
    void Update()
    {
        // Get the current speed from the NavMeshAgent
        float speed = navMeshAgent.velocity.magnitude;

        // Set the Speed parameter in the Animator
        animator.SetFloat("Speed", speed);

        // Optionally, you can control other NPC behaviors or animations here as well
        animator.speed = navMeshAgent.speed / 3f;
    }
}
