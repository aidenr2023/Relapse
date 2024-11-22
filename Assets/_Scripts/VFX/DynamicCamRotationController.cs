using Cinemachine;
using UnityEngine;

public class DynamicCamRotationController : MonoBehaviour, IDebugged
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera;

    [SerializeField] private float wallRunningTileAngle = 30f;

    [SerializeField] private CountdownTimer tiltTimer = new(0, true, true);

    private PlayerWallRunning _playerWallRunning;
    private CinemachineRecomposer _recomposer;
    private int _wallRunningDirection;

    private void Start()
    {
        // Get the components
        GetComponents();

        // Initialize the events
        InitializeEvents();

        // Add this script to the debug manager
        DebugManager.Instance.AddDebuggedObject(this);
    }

    private void GetComponents()
    {
        // Get the PlayerWallRunning component
        _playerWallRunning = GetComponent<PlayerWallRunning>();

        // Get the recomposer component
        _recomposer = virtualCamera.gameObject.GetComponent<CinemachineRecomposer>();
    }

    private void InitializeEvents()
    {
        _playerWallRunning.OnWallRunStart += OnWallRunStart;

        _playerWallRunning.OnWallRunEnd += OnWallRunEnd;
    }

    private void OnWallRunStart(PlayerWallRunning obj)
    {
        // Determine which direction the player is wall running
        if (_playerWallRunning.IsWallRunningRight)
            _wallRunningDirection = 1;

        else if (_playerWallRunning.IsWallRunningLeft)
            _wallRunningDirection = -1;

        // // Reset all timers
        // ResetAllTimers();
    }

    private void OnWallRunEnd(PlayerWallRunning obj)
    {
    }

    private void ResetAllTimers()
    {
        // Reset the tilt on timer
        tiltTimer.Reset();
    }

    private void Update()
    {
        // Update the timers
        var deltaMultiplier = _playerWallRunning.IsWallRunning ? 1 : -1;

        tiltTimer.Update(Time.deltaTime * deltaMultiplier);

        // Update the camera tilt
        UpdateCameraTilt();
    }

    private void UpdateCameraTilt()
    {
        // Return if the wall running script is null
        if (_playerWallRunning == null)
            return;

        // Calculate the angle of the camera tilt
        var tiltAngle = tiltTimer.OutputValue * wallRunningTileAngle;

        // Set the camera's rotation
        _recomposer.m_Dutch = tiltAngle * _wallRunningDirection;
    }

    public string GetDebugText()
    {
        return $"Dynamic Camera Rotation\n" +
               $"\tTilt On: {tiltTimer.OutputValue}\n";
    }
}