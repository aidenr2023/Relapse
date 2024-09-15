using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Dash : MonoBehaviour
{
    public float dashForce = 20f; // The strength of the dash impulse
    public float dashCooldown = 1f; // Cooldown time before the player can dash again

    private Rigidbody rb; // Reference to the player's Rigidbody
    private PlayerControls playerInputActions; // Reference to the Input System controls
    public bool canDash = true; // Whether the player can dash

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInputActions = new PlayerControls();
    }

    void OnEnable()
    {
        playerInputActions.GamePlay.dash.performed += OnDashPerformed;
        playerInputActions.Enable();
    }

    void OnDisable()
    {
        playerInputActions.GamePlay.dash.performed -= OnDashPerformed;
        playerInputActions.Disable();
    }

    private void OnDashPerformed(InputAction.CallbackContext context)
    {
        if (canDash)
        {
            Vector3 dashDirection = GetDashDirection();
            rb.AddForce(dashDirection * dashForce, ForceMode.Impulse); // Apply dash impulse
            StartCoroutine(DashCooldownCoroutine());
        }
    }

    private Vector3 GetDashDirection()
    {
        Vector3 currentVelocity = rb.velocity;
        Vector3 horizontalVelocity = new Vector3(currentVelocity.x, 0, currentVelocity.z);

        // If the player is stationary, default to forward
        if (horizontalVelocity.magnitude == 0)
        {
            return transform.forward;
        }

        return horizontalVelocity.normalized;
    }


    private IEnumerator DashCooldownCoroutine()
    {
        canDash = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true; // Allow dashing again after cooldown
    }
}
