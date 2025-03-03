using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFlinchStateHandler : StateMachineBehaviour
{
    [SerializeField] private int numberOfFlinchAnimations = 2; // Set this in Inspector
    private static readonly int HitBlendHash = Animator.StringToHash("HitBlend");

    /// <summary>
    /// handles the mixing of the flinch animations
    /// </summary>
    /// <param name="animator"></param>
    /// <param name="stateInfo"></param>
    /// <param name="layerIndex"></param>

    // Called when the flinch state is entered
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Randomize the flinch animation
        float randomBlendValue = Random.Range(0, numberOfFlinchAnimations);
        animator.SetFloat(HitBlendHash, randomBlendValue);
    }

    // Optional: Reset parameters when exiting the flinch state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Reset blend value if needed (e.g., to avoid animation "freezing")
        animator.SetFloat(HitBlendHash, 0); 
    }
}
