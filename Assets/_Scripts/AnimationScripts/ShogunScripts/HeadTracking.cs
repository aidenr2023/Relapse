using UnityEngine;

public class HeadTracking : MonoBehaviour
{
    #region Serialized Fields
    [SerializeField][Range(0,1)] public float lookAtWeight = 1.0f;
    [SerializeField] float headHeight = 1.5f;
    [SerializeField][Range(0,10)] float smoothSpeed = 10f;
    [SerializeField] private GameObject targetPosition;
    
    // Hip/Body tracking parameters
    [SerializeField][Range(0,10)] float bodyRotationSpeed = 5f;
    [SerializeField] private bool trackBody = true;
    #endregion

    #region Private Fields
    private Animator animator;
    private Vector3 smoothLookAtPosition;
    #endregion

    void Start()
    {
        targetPosition.transform.position = new Vector3(transform.position.x, transform.position.y + headHeight, transform.position.z);
        animator = GetComponent<Animator>();
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (animator)
        {
            // Head tracking
            smoothLookAtPosition = Vector3.Lerp(
                smoothLookAtPosition,
                targetPosition.transform.position,
                Time.deltaTime * smoothSpeed
            );

            animator.SetLookAtWeight(lookAtWeight);
            animator.SetLookAtPosition(smoothLookAtPosition);

            // Body (hip) rotation tracking
            if (trackBody)
            {
                Vector3 bodyLookDir = targetPosition.transform.position - transform.position;
                bodyLookDir.y = 0; // Optional: Remove vertical component

                if (bodyLookDir != Vector3.zero)
                {
                    Quaternion targetBodyRotation = Quaternion.LookRotation(bodyLookDir);
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