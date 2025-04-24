using UnityEngine;
using Cinemachine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class MenuCameraStateController : MonoBehaviour
{
    public enum MenuState { MainMenu, Credits }

    [Header("Camera Settings")]
    [SerializeField] private CinemachineVirtualCamera _mainMenuCamera;
    [SerializeField] private CinemachineVirtualCamera _creditsCamera;
    [SerializeField] private float _transitionSpeed = 1f;
    // global volume 
    [SerializeField] GameObject _globalVolume;
    
    
    [Header("UI Settings")]
    [SerializeField] private GameObject _mainCanvas;
    [SerializeField] private Button _creditsButton;
    [SerializeField] private Button _backButton;

    private MenuState _currentState = MenuState.MainMenu;

    private void Start()
    {
        ConfigureCinemachineBrain();
        InitializeState();
        SetupButtonCallbacks();
    }

    private void Update()
    {
        HandleEscapeInput();
    }

    private void ConfigureCinemachineBrain()
    {
        var brain = Camera.main.GetComponent<CinemachineBrain>();
        if (brain != null)
        {
            brain.m_DefaultBlend.m_Time = _transitionSpeed;
        }
    }

    private void InitializeState()
    {
        SetState(MenuState.MainMenu);
    }

    private void SetupButtonCallbacks()
    {
        _creditsButton.onClick.AddListener(() => SetState(MenuState.Credits));
        _backButton.onClick.AddListener(() => SetState(MenuState.MainMenu));
    }

    private void HandleEscapeInput()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (_currentState == MenuState.Credits)
            {
                SetState(MenuState.MainMenu);
            }
        }
    }

    public void SetState(MenuState newState)
    {
        if (_currentState == newState) return;

        _currentState = newState;

        switch (newState)
        {
            case MenuState.MainMenu:
                SetCameraPriorities(-1000, -2000);
                SetMainUI(true);
                //set the back button to be inactive
                _backButton.gameObject.SetActive(false);
                //set global to active
                _globalVolume.SetActive(true);
                break;

            case MenuState.Credits:
                SetCameraPriorities(-2000, -1000);
                SetMainUI(false);
                //set the back button to be active
                _backButton.gameObject.SetActive(true);
                //set global to inactive
                _globalVolume.SetActive(false);
                break;
        }
    }

    private void SetCameraPriorities(int mainPriority, int creditsPriority)
    {
        _mainMenuCamera.Priority = mainPriority;
        _creditsCamera.Priority = creditsPriority;
    }

    private void SetMainUI(bool visible)
    {
        if (_mainCanvas != null)
        {
            _mainCanvas.SetActive(visible);
        }
    }
}