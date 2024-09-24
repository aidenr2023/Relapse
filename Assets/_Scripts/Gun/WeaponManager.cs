using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponManager : MonoBehaviour
{
    #region Fields

    private TestPlayer _player;
    private Playerinfo _playerInfo;

    [SerializeField] private GunHolder gunHolder;

    #endregion

    #region Getters

    public TestPlayer Player => _player;
    public Playerinfo PlayerInfo => _playerInfo;
    public IGun CurrentGun => gunHolder.CurrentGun;

    #endregion

    #region Initialization Functions

    private void Awake()
    {
        // Initialize the components
        GetComponents();
    }

    private void Start()
    {
        // Initialize the input
        InitializeInput();
    }

    private void GetComponents()
    {
        // Get the TestPlayer component
        _player = GetComponent<TestPlayer>();

        // Get the Player info component
        _playerInfo = GetComponent<Playerinfo>();
    }

    private void InitializeInput()
    {
        // Initialize the input
        InputManager.Instance.PlayerControls.GamePlay.Shoot.performed += OnShoot;
    }

    private void OnShoot(InputAction.CallbackContext obj)
    {
        // Fire the IGun
        CurrentGun.Fire(this, transform.position, transform.forward);
    }

    #endregion
}