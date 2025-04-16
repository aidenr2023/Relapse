using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Processors;

[Serializable]
public struct UserSettings
{
    #region Constants

    public const float MIN_SENSITIVITY = 0.01f;
    public const float MAX_SENSITIVITY = 1;

    public const float MIN_GAMMA = -1;
    public const float MAX_GAMMA = 1;
    public const float MIN_VOLUME = -80;
    public const float MAX_VOLUME = 20;

    #endregion

    [field: SerializeField] public AudioMixer AudioMixer { get; private set; }

    #region Getters for Settings

    #region Input Settings

    [field: SerializeField, Header("Input Settings")]
    public Vector2 MouseSens { get; private set; }

    [field: SerializeField] public Vector2 ControllerSens { get; private set; }

    [field: SerializeField] public float MinimumLookDeadzone { get; set; }

    [field: SerializeField] public float MaximumLookDeadzone { get; set; }

    [field: SerializeField] public float MinimumMoveDeadzone { get; set; }

    [field: SerializeField] public float MaximumMoveDeadzone { get; set; }

    #endregion

    #region Volume Settings

    [field: SerializeField, Header("Volume Settings")]
    public float MasterVolume { get; private set; }

    [field: SerializeField] public float MusicVolume { get; private set; }
    [field: SerializeField] public float GameSfxVolume { get; private set; }
    [field: SerializeField] public float PlayerVolume { get; private set; }
    [field: SerializeField] public float EnemiesVolume { get; private set; }
    [field: SerializeField] public float OtherVolume { get; private set; }
    [field: SerializeField] public float UISfxVolume { get; private set; }

    #endregion

    #region Display Settings

    [field: SerializeField, Header("Display Settings")]
    public float Gamma { get; private set; }

    [field: SerializeField] public float MotionBlur { get; private set; }

    #endregion

    #region Accessibility Settings

    [field: SerializeField, Header("Accessibility Settings")]
    public float ShootFovMultiplier { get; private set; }

    #endregion

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

    public void SetMotionBlur(float value)
    {
        MotionBlur = Mathf.Clamp(value, 0, 1);
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

    public void SetSoundGameSfxVolume(float value)
    {
        GameSfxVolume = Mathf.Clamp(value, MIN_VOLUME, MAX_VOLUME);
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

    public void SetSoundUISfxVolume(float value)
    {
        UISfxVolume = Mathf.Clamp(value, MIN_VOLUME, MAX_VOLUME);
    }

    #endregion

    public void CopySettingsFrom(UserSettings other)
    {
        // // Copy the audio mixer reference
        // AudioMixer = other.AudioMixer;
        
        // Sensitivity settings
        MouseSens = other.MouseSens;
        ControllerSens = other.ControllerSens;
        MinimumLookDeadzone = other.MinimumLookDeadzone;
        MaximumLookDeadzone = other.MaximumLookDeadzone;
        MinimumMoveDeadzone = other.MinimumMoveDeadzone;
        MaximumMoveDeadzone = other.MaximumMoveDeadzone;

        // Sound Settings
        MasterVolume = other.MasterVolume;
        MusicVolume = other.MusicVolume;
        GameSfxVolume = other.GameSfxVolume;
        PlayerVolume = other.PlayerVolume;
        EnemiesVolume = other.EnemiesVolume;
        OtherVolume = other.OtherVolume;
        UISfxVolume = other.UISfxVolume;

        // Display Settings
        Gamma = other.Gamma;
        MotionBlur = other.MotionBlur;

        // Accessibility Settings
        ShootFovMultiplier = other.ShootFovMultiplier;
    }

    /// <summary>
    /// Some settings require more than just a value to be set, so this method is used to apply those settings.
    /// </summary>
    public void ApplySettings()
    {
        // Apply the deadzone settings
        InputManager.Instance.PControls.Player.LookController.ApplyParameterOverride(
            (StickDeadzoneProcessor d) => d.min, MinimumLookDeadzone
        );
        InputManager.Instance.PControls.Player.LookController.ApplyParameterOverride(
            (StickDeadzoneProcessor d) => d.max, MaximumLookDeadzone
        );
        InputManager.Instance.PControls.PlayerMovementBasic.Move.ApplyParameterOverride(
            (StickDeadzoneProcessor d) => d.min, MinimumMoveDeadzone
        );
        InputManager.Instance.PControls.PlayerMovementBasic.Move.ApplyParameterOverride(
            (StickDeadzoneProcessor d) => d.max, MaximumMoveDeadzone
        );

        // Apply the settings to the volume mixer
        ApplyVolumeMixerSettings();
    }

    private void ApplyVolumeMixerSettings()
    {
        SetVolumeSetting("MasterVolume", MasterVolume);
        SetVolumeSetting("MusicVolume", MusicVolume);
        SetVolumeSetting("GameSFXVolume", GameSfxVolume);
        SetVolumeSetting("PlayerVolume", PlayerVolume);
        SetVolumeSetting("EnemiesVolume", EnemiesVolume);
        SetVolumeSetting("OtherVolume", OtherVolume);
        SetVolumeSetting("UISFXVolume", UISfxVolume);
    }

    private void SetVolumeSetting(string settingName, float value)
    {
        AudioMixer.SetFloat(settingName, Mathf.Log10(value) * 20);
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }
}