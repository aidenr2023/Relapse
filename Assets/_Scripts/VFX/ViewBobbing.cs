using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;

public class ViewBobbing : MonoBehaviour
{
    #region Serialized Fields

    // reference to the virtual cam
    [SerializeField] private CinemachineCameraOffset virtualCam;

    // How long (in seconds) it takes the camera to go from bob to rest.
    [SerializeField] [Min(0)] private float transitionSpeed = .5f;

    // how quickly the player's head bobs.
    [SerializeField] [Min(0)] private float bobSpeed = 4.8f;

    // how dramatic the bob is.
    [SerializeField] [Min(0)] private float bobAmount = 0.05f;

    // How smooth the lerping on the bob is.
    [SerializeField] [Range(0, 1)] private float bobSmoothness = 0.95f;

    #endregion

    #region Private Fields

    // When sin is 1
    private float _timer = Mathf.PI / 2;

    private BasicPlayerMovement _playerMovement;

    #endregion

    #region Getters

    private bool IsViewBobbingActive =>
        _playerMovement.ParentComponent.CurrentMovementScript == _playerMovement &&
        _playerMovement.ParentComponent.IsGrounded &&
        _playerMovement.MovementInput != Vector2.zero;

    #endregion

    private void Awake()
    {
        // Get the player movement script
        _playerMovement = GetComponent<BasicPlayerMovement>();
    }

    private void Update()
    {
        if (IsViewBobbingActive)
        {
            // Make the bob faster if the player is sprinting
            var sprintMult = _playerMovement.IsSprinting ? _playerMovement.SprintMultiplier : 1;
            var currentBobSpeed = bobSpeed * sprintMult;

            // Make a timer for the bob speed
            _timer += currentBobSpeed * Time.deltaTime;

            // absolute value of y for a parabolic path
            var yValue = Vector3.zero.y + Mathf.Abs((Mathf.Sin(_timer) * bobAmount));

            // Where the camera should be
            var desiredOffset = new Vector3(
                Mathf.Cos(_timer) * bobAmount,
                yValue,
                Vector3.zero.z
            );

            // Lerp the current offset with the previous offset.
            // This is so we have a smooth transition from  stopping to walking.
            var currentOffset = Vector3.Lerp(virtualCam.m_Offset, desiredOffset, bobSmoothness);

            // Set the virtual cam offset
            virtualCam.m_Offset = currentOffset;
        }
        else
        {
            // reinitialize
            _timer = Mathf.PI / 2;

            // transition smoothly from walking to stopping.
            virtualCam.m_Offset = Vector3.Lerp(virtualCam.m_Offset, Vector3.zero, bobSmoothness);
        }

        // Avoid timer bloat
        // completed a full cycle on the unit circle. Reset to 0 to avoid bloated values.
        _timer %= (Mathf.PI * 2);
    }
}