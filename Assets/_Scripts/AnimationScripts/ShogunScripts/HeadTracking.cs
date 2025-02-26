using UnityEngine;

public class EnemyHeadTracking : MonoBehaviour
{
    /// <summary>
    /// This script is used to make the enemy's head & hip track the player
    /// </summary>
    #region Serialized Fields
    [SerializeField][Range(0,1)] private float lookAtWeight = 1.0f;
    [SerializeField] private float headHeight = 1.5f;
    [SerializeField][Range(0,10)] private float smoothSpeed = 10f;
    // Assign the target (e.g., player's transform) in the Inspector
    [SerializeField] private Transform target;

    // Hip/Body tracking parameters
    [SerializeField][Range(0,10)] private float bodyRotationSpeed = 5f;
    [SerializeField] private bool trackBody = true;
    #endregion

    #region Private Fields
    private Animator animator;
    private Vector3 smoothLookAtPosition;
    #endregion

    void Start()
    {
        animator = GetComponent<Animator>();

        // Optionally adjust the target's position to be at head height.
        if (target == null && Player.Instance != null)
        {
            target = Player.Instance.transform;
        }
        
        {
            target.position = new Vector3(target.position.x, transform.position.y + headHeight, target.position.z);
        }
    }
    
    //using Unity Mechanim's IK system to make the enemy's head track the player
    void OnAnimatorIK(int layerIndex)
    {
        if (animator && target != null)
        {
            // Smooth the look-at position for head tracking.
            smoothLookAtPosition = Vector3.Lerp(
                smoothLookAtPosition,
                target.position,
                Time.deltaTime * smoothSpeed
            );

            animator.SetLookAtWeight(lookAtWeight);
            animator.SetLookAtPosition(smoothLookAtPosition);

            // Body (hip) rotation tracking: make the enemy's upper body turn toward the target.
            if (trackBody)
            {
                // Calculate direction to target, ignoring vertical differences.
                Vector3 bodyLookDir = target.position - transform.position;
                bodyLookDir.y = 0;

                if (bodyLookDir != Vector3.zero)
                {
                    Quaternion targetBodyRotation = Quaternion.LookRotation(bodyLookDir);

                    // Smoothly interpolate the IK-driven body rotation toward the target rotation.
                    animator.bodyRotation = Quaternion.Slerp(
                        animator.bodyRotation,
                        targetBodyRotation,
                        Time.deltaTime * bodyRotationSpeed
                    );
                }
            }
        }
    }
}
