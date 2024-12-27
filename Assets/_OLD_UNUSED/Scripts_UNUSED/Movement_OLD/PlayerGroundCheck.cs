using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFallDetection : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private float cameraShakeIntensity = 5f; // The intensity of the camera shake
    [SerializeField] private float cameraShakeDuration = 0.1f; // The duration of the camera shake

    [SerializeField] private Sound fallSound;

    #endregion


    // Reference to the Rigidbody component
    private Rigidbody _rb;

    // To check if the player is falling
    private bool _isFalling = false;

    // Tracks how long the player has been falling
    private float _fallTime = 0f;

    // The time threshold for triggering the camera shake
    private float _fallThreshold = 0.5f;

    private void Start()
    {
        // Get the Rigidbody component
        _rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // Check if the player is falling (negative Y velocity)
        if (_rb.velocity.y < 0)
        {
            // If the player is falling, start counting the fall time
            if (!_isFalling)
            {
                _isFalling = true;
                _fallTime = 0f; // Reset the fall timer
            }

            // Increment fall time
            _fallTime += Time.deltaTime;
        }
        // If the player is not falling, reset the fall state
        else if (_isFalling)
            _isFalling = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the player has landed on something (anything counts as ground)
        if (!_isFalling)
            return;

        // If the player has been falling for more than the threshold, trigger the camera shake
        if (_fallTime > _fallThreshold)
        {
            // Call the camera shake method with intensity 5f and duration 0.1f
            CinemachineShake.Instance.ShakeCamera(cameraShakeIntensity, cameraShakeDuration);

            // Play the fall sound
            SoundManager.Instance.PlaySfx(fallSound);
        }

        // Reset fall-related variables
        _isFalling = false;
        _fallTime = 0f;
    }
}