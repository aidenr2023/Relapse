using UnityEngine;
using UnityEngine.Playables;
using Cinemachine;

/// <summary>
/// Switches camera states (via an Animator) based on cutscene events from the CutsceneHandler.
/// Listens to OnCutsceneStart and OnCutsceneEnd to change camera views.
/// </summary>
public class CutsceneCamSwitcher : MonoBehaviour
{
    //[SerializeField] private Animator _animator;
    
    [SerializeField]private CinemachineVirtualCamera _virtualCameraPlayer;
    [SerializeField]private CinemachineVirtualCamera _virtualCameraCutscene;
    
    // Reference to the CutsceneHandler to subscribe to its events.
    public CutsceneHandler cutsceneHandler;

    private CameraState _currentCameraState = CameraState.MainCamera;
    
    // Define available camera states.
    private enum CameraState
    {
        MainCamera,
        CutsceneCamera,
        CreditsCamera,
        MainMenuCamera,
    }
    
    private void OnEnable()
    {
        if(cutsceneHandler != null)
        {
            // Subscribe to cutscene events to trigger camera switches.
            cutsceneHandler.OnCutsceneStart.AddListener(SwitchToCutsceneCamera);
            cutsceneHandler.OnCutsceneEnd.AddListener(SwitchToMainCamera);
        }
    }
    
    private void OnDisable()
    {
        if(cutsceneHandler != null)
        {
            // Unsubscribe from events to avoid memory leaks.
            cutsceneHandler.OnCutsceneStart.RemoveListener(SwitchToCutsceneCamera);
            cutsceneHandler.OnCutsceneEnd.RemoveListener(SwitchToMainCamera);
        }
    }
    
    /// <summary>
    /// Centralized logic for switching the camera state via the Animator.
    /// </summary>
    /// <param name="newState">The new camera state to switch to.</param>
    private void SetCameraState(CameraState newState)
    {
        // Only switch if the new state is different.
        if (_currentCameraState == newState) return;

        switch (newState)
        {
            case CameraState.MainCamera:
                //log main camera
                Debug.Log("Main Camera");
                _virtualCameraPlayer.Priority = 10;
                //cutscene camera is no longer active
                _virtualCameraCutscene.Priority = 0;
                break;
            case CameraState.CutsceneCamera:
                //log cutscene camera
                Debug.Log("Cutscene Camera");
                _virtualCameraCutscene.Priority = 10;
                //player camera is no longer active
                _virtualCameraPlayer.Priority = 0;
                //_animator.Play("CutsceneCam");
                break;
            case CameraState.CreditsCamera:
                //log credits camera
                Debug.Log("Credits Camera");
                //_animator.Play("CreditsCam");
                break;
            case CameraState.MainMenuCamera:
                //log main menu camera
                Debug.Log("Main Menu Camera");
                //_animator.Play("MainMenuCam");
                break;
        }

        _currentCameraState = newState;
    }
    
    /// <summary>
    /// Switches to the cutscene camera (triggered when a cutscene starts).
    /// </summary>
    public void SwitchToCutsceneCamera()
    {
        SetCameraState(CameraState.CutsceneCamera);
    }
        
    /// <summary>
    /// Switches back to the main camera (triggered when a cutscene ends).
    /// </summary>
    public void SwitchToMainCamera()
    {
        SetCameraState(CameraState.MainCamera);
    }
    
    // Optional: Methods to switch to credits or main menu cameras if needed.
    public void SwitchToCreditsCamera()
    {
        SetCameraState(CameraState.CreditsCamera);
    }
    
    public void SwitchToMainMenuCamera()
    {
        SetCameraState(CameraState.MainMenuCamera);
    }
}
