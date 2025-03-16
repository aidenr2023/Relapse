using UnityEngine;

public class EnemyHeadTracking : MonoBehaviour
{
    /// <summary>
    /// This script is used to make the enemy's head & hip track the player
    /// </summary>

    #region Serialized Fields

    [SerializeField] [Range(0, 1)] private float lookAtWeight = 1.0f;

    [SerializeField] private float headHeight = 1.5f;

    [SerializeField] [Range(0, 10)] private float smoothSpeed = 10f;

    // Assign the target (e.g., player's transform) in the Inspector
    [SerializeField] private Transform target;

    // Hip/Body tracking parameters
    [SerializeField] [Range(0, 10)] private float bodyRotationSpeed = 5f;
    [SerializeField] private bool trackBody = true;

    #endregion

    #region Private Fields

    private Animator _animator;
    private Vector3 _smoothLookAtPosition;

    #endregion

    private void Start()
    {
        _animator = GetComponent<Animator>();

        // Optionally adjust the target's position to be at head height.
        if (target == null && Player.Instance != null)
            target = Player.Instance.transform;

        /*
         * I commented this line out because it was not doing whatever it was supposed to.
         * It was accidentally moving the player's position to the enemy's head height.
         * This was causing a really weird bug where the player would jitter upward whenever an enemy spawned.
         */
        // target.position = new Vector3(target.position.x, transform.position.y + headHeight, target.position.z);
    }

    //using Unity Mechanim's IK system to make the enemy's head track the player
    private void OnAnimatorIK(int layerIndex)
    {
        if (_animator && target != null)
        {
            // Smooth the look-at position for head tracking.
            _smoothLookAtPosition = Vector3.Lerp(
                _smoothLookAtPosition,
                target.position,
                Time.deltaTime * smoothSpeed
            );

            _animator.SetLookAtWeight(lookAtWeight);
            _animator.SetLookAtPosition(_smoothLookAtPosition);

            // Body (hip) rotation tracking: make the enemy's upper body turn toward the target.
            if (trackBody)
            {
                // Calculate direction to target, ignoring vertical differences.
                var bodyLookDir = target.position - transform.position;
                bodyLookDir.y = 0;

                if (bodyLookDir != Vector3.zero)
                {
                    var targetBodyRotation = Quaternion.LookRotation(bodyLookDir);

                    // Smoothly interpolate the IK-driven body rotation toward the target rotation.
                    _animator.bodyRotation = Quaternion.Slerp(
                        _animator.bodyRotation,
                        targetBodyRotation,
                        Time.deltaTime * bodyRotationSpeed
                    );
                }
            }
        }
    }
}