using UnityEngine;

public class PlayerViewBobbing : ComponentScript<PlayerVirtualCameraController>
{
    #region Serialized Fields

    // how quickly the player's head bobs.
    [Space, SerializeField, Min(0)] private float bobSpeed = 4.8f;
    [SerializeField, Min(0)] private float sprintStrengthMultiplier = 1.5f;

    // How smooth the lerping on the bob is.
    [Space, SerializeField, Range(0, 1)] private float bobSmoothness = 0.95f;
    [SerializeField, Range(0, 1)] private float stopMovingSmoothness = 0.95f;


    // how dramatic the bob is.
    [Space, SerializeField, Min(0)] private float bobAmount = 0.05f;
    [SerializeField] private float xStrength = 1f;
    [SerializeField] private float yStrength = 1f;

    #endregion

    #region Private Fields

    private TokenManager<Vector3>.ManagedToken _viewBobbingToken;

    // When sin is 1
    private float _timer = Mathf.PI / 2;

    private BasicPlayerMovement _playerMovement;

    private PlayerWallRunning _playerWallRunning;

    #endregion

    #region Getters

    private bool IsViewBobbingActive
    {
        get
        {
            var playerMovementGrounded =
                _playerMovement.ParentComponent.CurrentMovementScript == _playerMovement &&
                _playerMovement.ParentComponent.IsGrounded;

            var wallRunningActive =
                _playerMovement.ParentComponent.CurrentMovementScript == _playerWallRunning &&
                _playerWallRunning.IsWallRunning;

            return (playerMovementGrounded || wallRunningActive) && _playerMovement.MovementInput != Vector2.zero;
        }
    }

    #endregion

    protected override void CustomAwake()
    {
        base.CustomAwake();

        // Get the player movement script
        _playerMovement = GetComponent<BasicPlayerMovement>();

        // Get the player wall running script
        _playerWallRunning = GetComponent<PlayerWallRunning>();
    }

    private void Start()
    {
        // Create the token
        _viewBobbingToken = ParentComponent.DynamicOffsetModule.OffsetTokens.AddToken(Vector3.zero, -1, true);
    }

    private void Update()
    {
        if (IsViewBobbingActive)
        {
            // Make the bob faster if the player is sprinting
            var sprintSpeedMult = _playerMovement.IsSprinting ? _playerMovement.SprintMultiplier : 1;
            var currentBobSpeed = bobSpeed * sprintSpeedMult;

            // If the player is sprinting, make the bob stronger
            var sprintStrengthMult = _playerMovement.IsSprinting ? sprintStrengthMultiplier : 1;

            // Make the bob stronger based on the magnitude of the input
            var moveMagnitudeMult = _playerMovement.MovementInput.magnitude;

            var currentBobAmount = bobAmount * sprintStrengthMult * moveMagnitudeMult;

            // Make a timer for the bob speed
            _timer += currentBobSpeed * Time.deltaTime;

            var xValue = Mathf.Cos(_timer) * currentBobAmount;

            // absolute value of y for a parabolic path
            var yValue = Mathf.Abs((Mathf.Sin(_timer) * currentBobAmount));

            // Where the camera should be
            var desiredOffset = new Vector3(
                xValue * xStrength,
                yValue * yStrength,
                0
            );

            // Lerp the current offset with the previous offset.
            // This is so we have a smooth transition from  stopping to walking.
            var currentOffset = Vector3.Lerp(_viewBobbingToken.Value, desiredOffset, bobSmoothness);

            // Set the virtual cam offset
            _viewBobbingToken.Value = currentOffset;
        }
        else
        {
            // reinitialize
            _timer = Mathf.PI / 2;

            // transition smoothly from walking to stopping.
            _viewBobbingToken.Value = Vector3.Lerp(_viewBobbingToken.Value, Vector3.zero, stopMovingSmoothness);
        }

        // Avoid timer bloat
        // completed a full cycle on the unit circle. Reset to 0 to avoid bloated values.
        _timer %= (Mathf.PI * 2);
    }
}