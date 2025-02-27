using System;
using UnityEngine;
using UnityEngine.UI;

public class SensitivitySettingsUI : MonoBehaviour
{
    #region Serialized Fields

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
        // Set the sliders to the current sensitivity values
        mouseSensXSlider.Value = UserSettings.Instance.MouseSens.x;
        mouseSensYSlider.Value = UserSettings.Instance.MouseSens.y;

        controllerSensXSlider.Value = UserSettings.Instance.ControllerSens.x;
        controllerSensYSlider.Value = UserSettings.Instance.ControllerSens.y;

        minLookDeadzoneSlider.Value = UserSettings.Instance.MinimumLookDeadzone;
        maxLookDeadzoneSlider.Value = UserSettings.Instance.MaximumLookDeadzone;

        minMoveDeadzoneSlider.Value = UserSettings.Instance.MinimumMoveDeadzone;
        maxMoveDeadzoneSlider.Value = UserSettings.Instance.MaximumMoveDeadzone;

        brightnessSlider.Value = UserSettings.Instance.Gamma;

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
        UserSettings.Instance.SetMouseSensitivity(mouseSensXSlider.Value, mouseSensYSlider.Value);
    }

    private void OnControllerSensChanged(float arg0)
    {
        // Set the controller sensitivity
        UserSettings.Instance.SetControllerSensitivity(controllerSensXSlider.Value, controllerSensYSlider.Value);
    }

    private void OnMinLookDeadzoneChanged(float arg0)
    {
        // Set the minimum look deadzone
        UserSettings.Instance.MinimumLookDeadzone = minLookDeadzoneSlider.Value;
    }

    private void OnMinMoveDeadzoneChanged(float arg0)
    {
        // Set the minimum move deadzone
        UserSettings.Instance.MinimumMoveDeadzone = minMoveDeadzoneSlider.Value;
    }

    private void OnBrightnessChanged(float arg0)
    {
        // Set the gamma value
        UserSettings.Instance.SetGamma(brightnessSlider.Value);
    }

    private void OnMaxMoveDeadzoneChanged(float arg0)
    {
        UserSettings.Instance.MaximumMoveDeadzone = maxMoveDeadzoneSlider.Value;
    }

    private void OnMaxLookDeadzoneChanged(float arg0)
    {
        UserSettings.Instance.MaximumLookDeadzone = maxLookDeadzoneSlider.Value;
    }
}