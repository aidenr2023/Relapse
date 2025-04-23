using System;
using UnityEngine;

public class SettingsHelper : MonoBehaviour
{
    [SerializeField] private UserSettingsVariable settingsMenuSettings;
    [SerializeField] private UserSettingsVariable userSettings;

    public event Action<SliderSettingType, float> OnSettingChanged;
    public event Action<SettingsHelper> OnReset;

    // private void Awake()
    // {
    //     OnSettingChanged += (settingType, value) => Debug.Log($"Setting {settingType} changed to {value}");
    // }

    #region General Functions

    public void SetSettingValue(float value, SliderSettingType settingType)
    {
        switch (settingType)
        {
            // input settings
            case SliderSettingType.MouseSensitivityX:
                ChangeMouseSensitivityX(value);
                break;

            case SliderSettingType.MouseSensitivityY:
                ChangeMouseSensitivityY(value);
                break;

            case SliderSettingType.ControllerSensitivityX:
                ChangeControllerSensitivityX(value);
                break;

            case SliderSettingType.ControllerSensitivityY:
                ChangeControllerSensitivityY(value);
                break;

            case SliderSettingType.ControllerMinLookDeadzone:
                ChangeControllerMinLookDeadzone(value);
                break;

            case SliderSettingType.ControllerMaxLookDeadzone:
                ChangeControllerMaxLookDeadzone(value);
                break;

            case SliderSettingType.ControllerMinMoveDeadzone:
                ChangeControllerMinMoveDeadzone(value);
                break;

            case SliderSettingType.ControllerMaxMoveDeadzone:
                ChangeControllerMaxMoveDeadzone(value);
                break;

            // Sound Settings
            case SliderSettingType.MasterVolume:
                ChangeMasterVolume(value);
                break;

            case SliderSettingType.MusicVolume:
                ChangeMusicVolume(value);
                break;

            case SliderSettingType.PlayerSfxVolume:
                ChangePlayerSfxVolume(value);
                break;

            case SliderSettingType.EnemySfxVolume:
                ChangeEnemySfxVolume(value);
                break;

            case SliderSettingType.OtherSfxVolume:
                ChangeOtherSfxVolume(value);
                break;

            case SliderSettingType.UiSfxVolume:
                ChangeUiSfxVolume(value);
                break;

            // Display Settings
            case SliderSettingType.Brightness:
                ChangeBrightness(value);
                break;

            case SliderSettingType.MotionBlur:
                ChangeMotionBlur(value);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(settingType), settingType, null);
        }
    }

    public static float GetSettingValue(SliderSettingType settingType, UserSettingsVariable settingsMenuSettings)
    {
        return settingType switch
        {
            // input settings
            SliderSettingType.MouseSensitivityX => settingsMenuSettings.value.MouseSens.x,
            SliderSettingType.MouseSensitivityY => settingsMenuSettings.value.MouseSens.y,

            SliderSettingType.ControllerSensitivityX => settingsMenuSettings.value.ControllerSens.x,
            SliderSettingType.ControllerSensitivityY => settingsMenuSettings.value.ControllerSens.y,

            SliderSettingType.ControllerMinLookDeadzone => settingsMenuSettings.value.MinimumLookDeadzone,
            SliderSettingType.ControllerMaxLookDeadzone => settingsMenuSettings.value.MaximumLookDeadzone,

            SliderSettingType.ControllerMinMoveDeadzone => settingsMenuSettings.value.MinimumMoveDeadzone,
            SliderSettingType.ControllerMaxMoveDeadzone => settingsMenuSettings.value.MaximumMoveDeadzone,

            // Sound
            SliderSettingType.MasterVolume => settingsMenuSettings.value.MasterVolume,
            SliderSettingType.MusicVolume => settingsMenuSettings.value.MusicVolume,
            SliderSettingType.PlayerSfxVolume => settingsMenuSettings.value.PlayerVolume,
            SliderSettingType.EnemySfxVolume => settingsMenuSettings.value.EnemiesVolume,
            SliderSettingType.OtherSfxVolume => settingsMenuSettings.value.OtherVolume,
            SliderSettingType.UiSfxVolume => settingsMenuSettings.value.UISfxVolume,

            // Display
            SliderSettingType.Brightness => settingsMenuSettings.value.Gamma,
            SliderSettingType.MotionBlur => settingsMenuSettings.value.MotionBlur,

            _ => throw new ArgumentOutOfRangeException(nameof(settingType), settingType, null)
        };
    }

    #endregion

    #region Actual Settings Functions

    #region Input Settings

    private void ChangeMouseSensitivityX(float value)
    {
        settingsMenuSettings.value.SetMouseSensitivity(value, settingsMenuSettings.value.MouseSens.y);

        OnSettingChanged?.Invoke(SliderSettingType.MouseSensitivityX, value);
    }

    public void ChangeMouseSensitivityY(float value)
    {
        settingsMenuSettings.value.SetMouseSensitivity(settingsMenuSettings.value.MouseSens.x, value);

        OnSettingChanged?.Invoke(SliderSettingType.MouseSensitivityY, value);
    }

    public void ChangeControllerSensitivityX(float value)
    {
        settingsMenuSettings.value.SetControllerSensitivity(value, settingsMenuSettings.value.ControllerSens.y);

        OnSettingChanged?.Invoke(SliderSettingType.ControllerSensitivityX, value);
    }

    public void ChangeControllerSensitivityY(float value)
    {
        settingsMenuSettings.value.SetControllerSensitivity(settingsMenuSettings.value.ControllerSens.x, value);

        OnSettingChanged?.Invoke(SliderSettingType.ControllerSensitivityY, value);
    }

    public void ChangeControllerMinLookDeadzone(float value)
    {
        settingsMenuSettings.value.MinimumLookDeadzone = value;

        OnSettingChanged?.Invoke(SliderSettingType.ControllerMinLookDeadzone, value);
    }

    public void ChangeControllerMaxLookDeadzone(float value)
    {
        settingsMenuSettings.value.MaximumLookDeadzone = value;

        OnSettingChanged?.Invoke(SliderSettingType.ControllerMaxLookDeadzone, value);
    }

    public void ChangeControllerMinMoveDeadzone(float value)
    {
        settingsMenuSettings.value.MinimumMoveDeadzone = value;

        OnSettingChanged?.Invoke(SliderSettingType.ControllerMinMoveDeadzone, value);
    }

    public void ChangeControllerMaxMoveDeadzone(float value)
    {
        settingsMenuSettings.value.MaximumMoveDeadzone = value;

        OnSettingChanged?.Invoke(SliderSettingType.ControllerMaxMoveDeadzone, value);
    }

    #endregion

    #region Sound Settings

    private void ChangeMasterVolume(float value)
    {
        settingsMenuSettings.value.SetSoundMasterVolume(value);

        OnSettingChanged?.Invoke(SliderSettingType.MasterVolume, value);
    }

    private void ChangeMusicVolume(float value)
    {
        settingsMenuSettings.value.SetSoundMusicVolume(value);

        OnSettingChanged?.Invoke(SliderSettingType.MusicVolume, value);
    }

    private void ChangePlayerSfxVolume(float value)
    {
        settingsMenuSettings.value.SetSoundPlayerVolume(value);

        OnSettingChanged?.Invoke(SliderSettingType.PlayerSfxVolume, value);
    }

    private void ChangeEnemySfxVolume(float value)
    {
        settingsMenuSettings.value.SetSoundEnemiesVolume(value);

        OnSettingChanged?.Invoke(SliderSettingType.EnemySfxVolume, value);
    }

    private void ChangeOtherSfxVolume(float value)
    {
        settingsMenuSettings.value.SetSoundOtherVolume(value);

        OnSettingChanged?.Invoke(SliderSettingType.OtherSfxVolume, value);
    }

    private void ChangeUiSfxVolume(float value)
    {
        settingsMenuSettings.value.SetSoundUISfxVolume(value);

        OnSettingChanged?.Invoke(SliderSettingType.UiSfxVolume, value);
    }

    #endregion

    #region Display Settings

    private void ChangeBrightness(float value)
    {
        settingsMenuSettings.value.SetGamma(value);

        OnSettingChanged?.Invoke(SliderSettingType.Brightness, value);
    }

    private void ChangeMotionBlur(float value)
    {
        settingsMenuSettings.value.SetMotionBlur(value);

        OnSettingChanged?.Invoke(SliderSettingType.MotionBlur, value);
    }

    #endregion

    #endregion

    public void InitializeMenuSettings()
    {
        settingsMenuSettings.value.CopySettingsFrom(userSettings.value);
    }

    public void SaveSettings()
    {
        // First, copy the settings from the settingsMenuSettings to the userSettings
        userSettings.value.CopySettingsFrom(settingsMenuSettings.value);

        // Then, apply the userSettings
        userSettings.value.ApplySettings();

        // Then, save the settings to disk
        SettingsLoader.Instance.SaveSettingsToDisk();
    }

    public void ResetSettings()
    {
        settingsMenuSettings.value.CopySettingsFrom(userSettings.defaultValue);
        
        // Invoke the OnReset event
        OnReset?.Invoke(this);
    }
}