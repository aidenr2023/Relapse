using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Dash : MonoBehaviour
{
    [SerializeField] [Range(0, 2000)] private float  dashForce = 100f; // The strength of the dash impulse
    [SerializeField] [Range(0,10)] private float dashCooldown = 1f; // Cooldown time before the player can dash again

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
    
    private void FixedUpdate()
    {
        ClampVerticalVelocity();
    }

    private void OnDashPerformed(InputAction.CallbackContext context)
    {
        if (canDash)
        {
            Vector3 dashDirection = GetDashDirection();
            
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            ClampVerticalVelocity();
            rb.AddForce(dashDirection * dashForce, ForceMode.Impulse); // Apply dash impulse
            StartCoroutine(DashCooldownCoroutine());
        }
    }

    private void ClampVerticalVelocity()
    {
        // Limit how much vertical velocity (Y-axis) the player can have
        if (rb.velocity.y > 5f) // Adjust this value to control vertical speed when dashing
        {
            rb.velocity = new Vector3(rb.velocity.x, 5f, rb.velocity.z);
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
