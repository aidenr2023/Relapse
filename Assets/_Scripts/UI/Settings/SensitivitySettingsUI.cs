using System;
using UnityEngine;
using UnityEngine.UI;

public class SensitivitySettingsUI : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private UserSettingsVariable settingsMenuSettings;
    
    #region Input Settings

    [SerializeField] private SettingsSlider mouseSensXSlider;
    [SerializeField] private SettingsSlider mouseSensYSlider;

    [SerializeField] private SettingsSlider controllerSensXSlider;
    [SerializeField] private SettingsSlider controllerSensYSlider;

    [SerializeField] private SettingsSlider minLookDeadzoneSlider;
    [SerializeField] private SettingsSlider maxLookDeadzoneSlider;

    [SerializeField] private SettingsSlider minMoveDeadzoneSlider;
    [SerializeField] private SettingsSlider maxMoveDeadzoneSlider;

    #endregion

    [SerializeField] private SettingsSlider brightnessSlider;

    #endregion

    private void Start()
    {
        return;
        
        // Set the sliders to the current sensitivity values
        mouseSensXSlider.Value = settingsMenuSettings.value.MouseSens.x;
        mouseSensYSlider.Value = settingsMenuSettings.value.MouseSens.y;

        controllerSensXSlider.Value = settingsMenuSettings.value.ControllerSens.x;
        controllerSensYSlider.Value = settingsMenuSettings.value.ControllerSens.y;

        minLookDeadzoneSlider.Value = settingsMenuSettings.value.MinimumLookDeadzone;
        maxLookDeadzoneSlider.Value = settingsMenuSettings.value.MaximumLookDeadzone;

        minMoveDeadzoneSlider.Value = settingsMenuSettings.value.MinimumMoveDeadzone;
        maxMoveDeadzoneSlider.Value = settingsMenuSettings.value.MaximumMoveDeadzone;

        brightnessSlider.Value = settingsMenuSettings.value.Gamma;

        // Initialize the slider events
        mouseSensXSlider.onValueChanged.AddListener(OnMouseSensChanged);
        mouseSensYSlider.onValueChanged.AddListener(OnMouseSensChanged);

        controllerSensXSlider.onValueChanged.AddListener(OnControllerSensChanged);
        controllerSensYSlider.onValueChanged.AddListener(OnControllerSensChanged);

        minLookDeadzoneSlider.onValueChanged.AddListener(OnMinLookDeadzoneChanged);
        maxLookDeadzoneSlider.onValueChanged.AddListener(OnMaxLookDeadzoneChanged);

        minMoveDeadzoneSlider.onValueChanged.AddListener(OnMinMoveDeadzoneChanged);
        maxMoveDeadzoneSlider.onValueChanged.AddListener(OnMaxMoveDeadzoneChanged);

        brightnessSlider.onValueChanged.AddListener(OnBrightnessChanged);
    }

    private void OnMouseSensChanged(float arg0)
    {
        // Set the mouse sensitivity
        settingsMenuSettings.value.SetMouseSensitivity(mouseSensXSlider.Value, mouseSensYSlider.Value);
    }

    private void OnControllerSensChanged(float arg0)
    {
        // Set the controller sensitivity
        settingsMenuSettings.value.SetControllerSensitivity(controllerSensXSlider.Value, controllerSensYSlider.Value);
    }

    private void OnMinLookDeadzoneChanged(float arg0)
    {
        // Set the minimum look deadzone
        settingsMenuSettings.value.MinimumLookDeadzone = minLookDeadzoneSlider.Value;
    }

    private void OnMinMoveDeadzoneChanged(float arg0)
    {
        // Set the minimum move deadzone
        settingsMenuSettings.value.MinimumMoveDeadzone = minMoveDeadzoneSlider.Value;
    }

    private void OnBrightnessChanged(float arg0)
    {
        // Set the gamma value
        settingsMenuSettings.value.SetGamma(brightnessSlider.Value);
    }

    private void OnMaxMoveDeadzoneChanged(float arg0)
    {
        settingsMenuSettings.value.MaximumMoveDeadzone = maxMoveDeadzoneSlider.Value;
    }

    private void OnMaxLookDeadzoneChanged(float arg0)
    {
        settingsMenuSettings.value.MaximumLookDeadzone = maxLookDeadzoneSlider.Value;
    }
}