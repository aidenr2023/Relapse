using System;
using UnityEngine;

public class SettingsHelper : MonoBehaviour
{
    public event Action<SliderSettingType, float> OnSettingChanged;

    private void Awake()
    {
        OnSettingChanged += (settingType, value) => Debug.Log($"Setting {settingType} changed to {value}");
    }

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

            default:
                throw new ArgumentOutOfRangeException(nameof(settingType), settingType, null);
        }
    }

    public static float GetSettingValue(SliderSettingType settingType)
    {
        return settingType switch
        {
            // input settings
            SliderSettingType.MouseSensitivityX => UserSettings.Instance.MouseSens.x,
            SliderSettingType.MouseSensitivityY => UserSettings.Instance.MouseSens.y,

            SliderSettingType.ControllerSensitivityX => UserSettings.Instance.ControllerSens.x,
            SliderSettingType.ControllerSensitivityY => UserSettings.Instance.ControllerSens.y,

            SliderSettingType.ControllerMinLookDeadzone => UserSettings.Instance.MinimumLookDeadzone,
            SliderSettingType.ControllerMaxLookDeadzone => UserSettings.Instance.MaximumLookDeadzone,

            SliderSettingType.ControllerMinMoveDeadzone => UserSettings.Instance.MinimumMoveDeadzone,
            SliderSettingType.ControllerMaxMoveDeadzone => UserSettings.Instance.MaximumMoveDeadzone,

            // Sound
            SliderSettingType.MasterVolume => UserSettings.Instance.MasterVolume,
            SliderSettingType.MusicVolume => UserSettings.Instance.MusicVolume,
            SliderSettingType.PlayerSfxVolume => UserSettings.Instance.PlayerVolume,
            SliderSettingType.EnemySfxVolume => UserSettings.Instance.EnemiesVolume,
            SliderSettingType.OtherSfxVolume => UserSettings.Instance.OtherVolume,
            SliderSettingType.UiSfxVolume => UserSettings.Instance.UISFXVolume,

            // Display
            SliderSettingType.Brightness => UserSettings.Instance.Gamma,

            _ => throw new ArgumentOutOfRangeException(nameof(settingType), settingType, null)
        };
    }

    #endregion

    #region Actual Settings Functions

    #region Input Settings

    private void ChangeMouseSensitivityX(float value)
    {
        UserSettings.Instance.SetMouseSensitivity(value, UserSettings.Instance.MouseSens.y);

        OnSettingChanged?.Invoke(SliderSettingType.MouseSensitivityX, value);
    }

    public void ChangeMouseSensitivityY(float value)
    {
        UserSettings.Instance.SetMouseSensitivity(UserSettings.Instance.MouseSens.x, value);

        OnSettingChanged?.Invoke(SliderSettingType.MouseSensitivityY, value);
    }

    public void ChangeControllerSensitivityX(float value)
    {
        UserSettings.Instance.SetControllerSensitivity(value, UserSettings.Instance.ControllerSens.y);

        OnSettingChanged?.Invoke(SliderSettingType.ControllerSensitivityX, value);
    }

    public void ChangeControllerSensitivityY(float value)
    {
        UserSettings.Instance.SetControllerSensitivity(UserSettings.Instance.ControllerSens.x, value);

        OnSettingChanged?.Invoke(SliderSettingType.ControllerSensitivityY, value);
    }

    public void ChangeControllerMinLookDeadzone(float value)
    {
        UserSettings.Instance.MinimumLookDeadzone = value;

        OnSettingChanged?.Invoke(SliderSettingType.ControllerMinLookDeadzone, value);
    }

    public void ChangeControllerMaxLookDeadzone(float value)
    {
        UserSettings.Instance.MaximumLookDeadzone = value;

        OnSettingChanged?.Invoke(SliderSettingType.ControllerMaxLookDeadzone, value);
    }

    public void ChangeControllerMinMoveDeadzone(float value)
    {
        UserSettings.Instance.MinimumMoveDeadzone = value;

        OnSettingChanged?.Invoke(SliderSettingType.ControllerMinMoveDeadzone, value);
    }

    public void ChangeControllerMaxMoveDeadzone(float value)
    {
        UserSettings.Instance.MaximumMoveDeadzone = value;

        OnSettingChanged?.Invoke(SliderSettingType.ControllerMaxMoveDeadzone, value);
    }

    #endregion

    #region Sound Settings

    private void ChangeMasterVolume(float value)
    {
        UserSettings.Instance.SetSoundMasterVolume(value);

        OnSettingChanged?.Invoke(SliderSettingType.MasterVolume, value);
    }

    private void ChangeMusicVolume(float value)
    {
        UserSettings.Instance.SetSoundMusicVolume(value);

        OnSettingChanged?.Invoke(SliderSettingType.MusicVolume, value);
    }

    private void ChangePlayerSfxVolume(float value)
    {
        UserSettings.Instance.SetSoundPlayerVolume(value);

        OnSettingChanged?.Invoke(SliderSettingType.PlayerSfxVolume, value);
    }

    private void ChangeEnemySfxVolume(float value)
    {
        UserSettings.Instance.SetSoundEnemiesVolume(value);

        OnSettingChanged?.Invoke(SliderSettingType.EnemySfxVolume, value);
    }

    private void ChangeOtherSfxVolume(float value)
    {
        UserSettings.Instance.SetSoundMusicVolume(value);

        OnSettingChanged?.Invoke(SliderSettingType.OtherSfxVolume, value);
    }

    private void ChangeUiSfxVolume(float value)
    {
        UserSettings.Instance.SetSoundUISFXVolume(value);

        OnSettingChanged?.Invoke(SliderSettingType.UiSfxVolume, value);
    }

    #endregion

    #region Display Settings

    private void ChangeBrightness(float value)
    {
        UserSettings.Instance.SetGamma(value);

        OnSettingChanged?.Invoke(SliderSettingType.Brightness, value);
    }

    #endregion

    #endregion
}