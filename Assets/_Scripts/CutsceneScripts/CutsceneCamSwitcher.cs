using UnityEngine;
using UnityEngine.Playables;
using Cinemachine;

/// <summary>
/// Switches camera states based on cutscene events from the CutsceneHandler.
/// Uses a single combined function on cutscene start to determine whether to use the cutscene camera
/// or the FPS cutscene camera, based on the IsCamChangeNeeded flag in the assigned triggers.
/// </summary>
public class CutsceneCamSwitcher : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera _virtualCameraPlayer;
    [SerializeField] private CinemachineVirtualCamera _virtualCameraFPS;
    [SerializeField] private CinemachineVirtualCamera _virtualCameraCutscene;
    [SerializeField] private CutsceneTrigger[] _cutsceneTrigger;

    // Reference to the CutsceneHandler to subscribe to its events.
    public CutsceneHandler cutsceneHandler;

    private CameraState _currentCameraState = CameraState.MainCamera;

    // Define available camera states.
    private enum CameraState
    {
        MainCamera,
        CutsceneCamera,
        FPSCutsceneCamera,
        MainMenuCamera,
    }

    private void OnEnable()
    {
        if (cutsceneHandler != null)
        {
            // Subscribe to cutscene events.
            cutsceneHandler.OnCutsceneStart.AddListener(SwitchOnCutsceneStart);
            cutsceneHandler.OnCutsceneEnd.AddListener(SwitchToMainCamera);
        }
    }

    private void OnDisable()
    {
        if (cutsceneHandler != null)
        {
            // Unsubscribe to avoid memory leaks.
            cutsceneHandler.OnCutsceneStart.RemoveListener(SwitchOnCutsceneStart);
            cutsceneHandler.OnCutsceneEnd.RemoveListener(SwitchToMainCamera);
        }
    }

    /// <summary>
    /// Centralized logic for switching camera states.
    /// </summary>
    /// <param name="newState">The new camera state to switch to.</param>
    private void SetCameraState(CameraState newState)
    {
        // Only switch if the new state is different.
        if (_currentCameraState == newState) return;

        switch (newState)
        {
            case CameraState.MainCamera:
                Debug.Log("Switching to Main Camera");
                _virtualCameraPlayer.Priority = 10;
                _virtualCameraCutscene.Priority = 0;
                _virtualCameraFPS.Priority = 0;
                break;
            case CameraState.CutsceneCamera:
                Debug.Log("Switching to Cutscene Camera");
                _virtualCameraCutscene.Priority = 10;
                _virtualCameraPlayer.Priority = 0;
                _virtualCameraFPS.Priority = 0;
                break;
            case CameraState.FPSCutsceneCamera:
                Debug.Log("Switching to FPS Cutscene Camera");
                _virtualCameraFPS.Priority = 10;
                _virtualCameraCutscene.Priority = 0;
                _virtualCameraPlayer.Priority = 0;
                break;
            case CameraState.MainMenuCamera:
                Debug.Log("Switching to Main Menu Camera");
                // Add additional logic if needed.
                break;
        }

        _currentCameraState = newState;
    }

    /// <summary>
    /// Called on cutscene start. Checks the triggers and selects the appropriate camera state.
    /// </summary>
    public void SwitchOnCutsceneStart()
    {
        bool camChangeNeeded = false;
        // Iterate over all cutscene triggers.
        foreach (CutsceneTrigger trigger in _cutsceneTrigger)
        {
            if (trigger.IsCamChangeNeeded)
            {
                camChangeNeeded = true;
                break;
            }
        }

        // If any trigger requires a camera change, use the dedicated cutscene camera;
        // otherwise, use the FPS cutscene camera.
        if (camChangeNeeded)
        {
            SetCameraState(CameraState.CutsceneCamera);
        }
        else
        {
            SetCameraState(CameraState.FPSCutsceneCamera);
        }
    }

    /// <summary>
    /// Called on cutscene end to revert back to the main camera.
    /// </summary>
    public void SwitchToMainCamera()
    {
        SetCameraState(CameraState.MainCamera);
    }

    // Optional: Additional methods for other camera states.
    public void SwitchToMainMenuCamera()
    {
        SetCameraState(CameraState.MainMenuCamera);
    }
}
