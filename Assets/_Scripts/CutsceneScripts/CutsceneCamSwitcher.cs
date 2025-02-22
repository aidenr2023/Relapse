using UnityEngine;
using UnityEngine.Playables;
using Cinemachine;
using UnityEngine.SceneManagement;

/// <summary>
/// Switches camera states across multiple scenes, dynamically finding triggers in loaded scenes
/// </summary>
public class CutsceneCamSwitcher : MonoBehaviour
{
    [Header("Camera References")]
    [SerializeField] private CinemachineVirtualCamera _virtualCameraPlayer;
    [SerializeField] private CinemachineVirtualCamera _virtualCameraFPS;
    [SerializeField] private CinemachineVirtualCamera _virtualCameraCutscene;

    [Header("Dependencies")]
    [SerializeField] private CutsceneHandler cutsceneHandler;

    private CameraState _currentCameraState = CameraState.MainCamera;
    private CutsceneTrigger[] _cutsceneTriggers;

    private enum CameraState
    {
        MainCamera,
        CutsceneCamera,
        FPSCutsceneCamera,
        MainMenuCamera,
    }

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnEnable()
    {
        RefreshCutsceneTriggers();
        RegisterEventHandlers();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        UnregisterEventHandlers();
    }

    private void RegisterEventHandlers()
    {
        if (cutsceneHandler != null)
        {
            cutsceneHandler.OnCutsceneStart.AddListener(SwitchOnCutsceneStart);
            cutsceneHandler.OnCutsceneEnd.AddListener(SwitchToMainCamera);
        }
    }

    private void UnregisterEventHandlers()
    {
        if (cutsceneHandler != null)
        {
            cutsceneHandler.OnCutsceneStart.RemoveListener(SwitchOnCutsceneStart);
            cutsceneHandler.OnCutsceneEnd.RemoveListener(SwitchToMainCamera);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshCutsceneTriggers();
    }

    private void RefreshCutsceneTriggers()
    {
        _cutsceneTriggers = FindObjectsOfType<CutsceneTrigger>(true);
        Debug.Log($"Refreshed cutscene triggers. Found: {_cutsceneTriggers.Length}");
    }

    private void SetCameraState(CameraState newState)
    {
        if (_currentCameraState == newState) return;

        switch (newState)
        {
            case CameraState.MainCamera:
                SetCameraPriorities(_virtualCameraPlayer);
                break;
            case CameraState.CutsceneCamera:
                SetCameraPriorities(_virtualCameraCutscene);
                break;
            case CameraState.FPSCutsceneCamera:
                SetCameraPriorities(_virtualCameraFPS);
                break;
            case CameraState.MainMenuCamera:
                // Implement menu camera logic
                break;
        }

        _currentCameraState = newState;
        Debug.Log($"Switched to: {newState}");
    }

    private void SetCameraPriorities(CinemachineVirtualCamera activeCamera)
    {
        _virtualCameraPlayer.Priority = _virtualCameraPlayer == activeCamera ? 10 : 0;
        _virtualCameraCutscene.Priority = _virtualCameraCutscene == activeCamera ? 10 : 0;
        _virtualCameraFPS.Priority = _virtualCameraFPS == activeCamera ? 10 : 0;
    }

    public void SwitchOnCutsceneStart()
    {
        bool needsCameraChange = CheckTriggersForCameraNeeds();
        SetCameraState(needsCameraChange ? CameraState.CutsceneCamera : CameraState.FPSCutsceneCamera);
    }

    private bool CheckTriggersForCameraNeeds()
    {
        if (_cutsceneTriggers == null || _cutsceneTriggers.Length == 0) return false;

        foreach (var trigger in _cutsceneTriggers)
        {
            if (trigger != null && trigger.IsCamChangeNeeded)
            {
                return true;
            }
        }
        return false;
    }

    public void SwitchToMainCamera()
    {
        SetCameraState(CameraState.MainCamera);
    }

    public void SwitchToMainMenuCamera()
    {
        SetCameraState(CameraState.MainMenuCamera);
    }
}