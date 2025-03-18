using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Processors;

public class UserSettings
{
    #region Constants

    public const float MIN_SENSITIVITY = 0.01f;
    public const float MAX_SENSITIVITY = 1;

    public const float MIN_GAMMA = -1;
    public const float MAX_GAMMA = 1;
    public const float MIN_VOLUME = -80;
    public const float MAX_VOLUME = 20;

    #endregion

    #region Singleton Pattern

    private static UserSettings _instance;

    public static UserSettings Instance
    {
        get
        {
            if (_instance == null)
                _instance = new UserSettings();

            return _instance;
        }
    }

    #endregion

    #region Getters for Settings

    // Input settings

    public Vector2 MouseSens { get; private set; } = new(.5f, .5f);
    public Vector2 ControllerSens { get; private set; } = new(.5f, .5f);

    private float _minimumLookDeadzone;
    private float _maximumLookDeadzone;
    private float _minimumMoveDeadzone;
    private float _maximumMoveDeadzone;

    public float MinimumLookDeadzone
    {
        get => _minimumLookDeadzone;
        set
        {
            _minimumLookDeadzone = Mathf.Clamp(value, 0, 1);
            InputManager.Instance.PControls.Player.LookController.ApplyParameterOverride(
                (StickDeadzoneProcessor d) => d.min, _minimumLookDeadzone
            );
        }
    }

    public float MaximumLookDeadzone
    {
        get => _maximumLookDeadzone;
        set
        {
            _maximumLookDeadzone = Mathf.Clamp(value, 0, 1);
            InputManager.Instance.PControls.Player.LookController.ApplyParameterOverride(
                (StickDeadzoneProcessor d) => d.max, _maximumLookDeadzone
            );
        }
    }

    public float MinimumMoveDeadzone
    {
        get => _minimumMoveDeadzone;
        set
        {
            _minimumMoveDeadzone = Mathf.Clamp(value, 0, 1);
            InputManager.Instance.PControls.PlayerMovementBasic.Move.ApplyParameterOverride(
                (StickDeadzoneProcessor d) => d.min, _minimumMoveDeadzone
            );
        }
    }

    public float MaximumMoveDeadzone
    {
        get => _maximumMoveDeadzone;
        set
        {
            _maximumMoveDeadzone = Mathf.Clamp(value, 0, 1);
            InputManager.Instance.PControls.PlayerMovementBasic.Move.ApplyParameterOverride(
                (StickDeadzoneProcessor d) => d.max, _maximumMoveDeadzone
            );
        }
    }

    public float Gamma { get; private set; }

    // Volume settings
    public float MasterVolume { get; private set; }
    public float MusicVolume { get; private set; }
    public float GameSFXVolume { get; private set; }
    public float PlayerVolume { get; private set; }
    public float EnemiesVolume { get; private set; }
    public float OtherVolume { get; private set; }
    public float UISFXVolume { get; private set; }

    #endregion

    #region Public Methods

    public void SetMouseSensitivity(float x, float y)
    {
        x = Mathf.Clamp(x, MIN_SENSITIVITY, MAX_SENSITIVITY);
        y = Mathf.Clamp(y, MIN_SENSITIVITY, MAX_SENSITIVITY);

        MouseSens = new Vector2(
            Mathf.Clamp(x, MIN_SENSITIVITY, MAX_SENSITIVITY),
            Mathf.Clamp(y, MIN_SENSITIVITY, MAX_SENSITIVITY)
        );
    }

    public void SetControllerSensitivity(float x, float y)
    {
        x = Mathf.Clamp(x, MIN_SENSITIVITY, MAX_SENSITIVITY);
        y = Mathf.Clamp(y, MIN_SENSITIVITY, MAX_SENSITIVITY);

        ControllerSens = new Vector2(
            Mathf.Clamp(x, MIN_SENSITIVITY, MAX_SENSITIVITY),
            Mathf.Clamp(y, MIN_SENSITIVITY, MAX_SENSITIVITY)
        );
    }

    public void SetGamma(float value)
    {
        Gamma = Mathf.Clamp(value, MIN_GAMMA, MAX_GAMMA);
    }

    // Volume settings individual
    public void SetSoundMasterVolume(float value)
    {
        MasterVolume = Mathf.Clamp(value, MIN_VOLUME, MAX_VOLUME);
    }

    public void SetSoundMusicVolume(float value)
    {
        MusicVolume = Mathf.Clamp(value, MIN_VOLUME, MAX_VOLUME);
    }

    public void SetSoundGameSFXVolume(float value)
    {
        GameSFXVolume = Mathf.Clamp(value, MIN_VOLUME, MAX_VOLUME);
    }

    public void SetSoundPlayerVolume(float value)
    {
        PlayerVolume = Mathf.Clamp(value, MIN_VOLUME, MAX_VOLUME);
    }

    public void SetSoundEnemiesVolume(float value)
    {
        EnemiesVolume = Mathf.Clamp(value, MIN_VOLUME, MAX_VOLUME);
    }

    public void SetSoundOtherVolume(float value)
    {
        OtherVolume = Mathf.Clamp(value, MIN_VOLUME, MAX_VOLUME);
    }

    public void SetSoundUISFXVolume(float value)
    {
        UISFXVolume = Mathf.Clamp(value, MIN_VOLUME, MAX_VOLUME);
    }


    // This is probably better to use than the above methods
    public void SetAllVolumes(float Master, float Music, float GameSFX, float Player, float Enemies, float Other,
        float UISFX)
    {
        MasterVolume = Mathf.Clamp(Master, MIN_VOLUME, MAX_VOLUME);
        MusicVolume = Mathf.Clamp(Music, MIN_VOLUME, MAX_VOLUME);
        GameSFXVolume = Mathf.Clamp(GameSFX, MIN_VOLUME, MAX_VOLUME);
        PlayerVolume = Mathf.Clamp(Player, MIN_VOLUME, MAX_VOLUME);
        EnemiesVolume = Mathf.Clamp(Enemies, MIN_VOLUME, MAX_VOLUME);
        OtherVolume = Mathf.Clamp(Other, MIN_VOLUME, MAX_VOLUME);
        UISFXVolume = Mathf.Clamp(UISFX, MIN_VOLUME, MAX_VOLUME);
    }

    #endregion
}