using System;
using UnityEngine;
using UnityEngine.UI;

public class SensitivitySettingsUI : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private SettingsSlider mouseSensXSlider;
    [SerializeField] private SettingsSlider mouseSensYSlider;

    [SerializeField] private SettingsSlider controllerSensXSlider;
    [SerializeField] private SettingsSlider controllerSensYSlider;

    #endregion

    private void Start()
    {
        // Set the sliders to the current sensitivity values
        mouseSensXSlider.Value = UserSettings.Instance.MouseSens.x;
        mouseSensYSlider.Value = UserSettings.Instance.MouseSens.y;
        controllerSensXSlider.Value = UserSettings.Instance.ControllerSens.x;
        controllerSensYSlider.Value = UserSettings.Instance.ControllerSens.y;

        // Initialize the slider events
        mouseSensXSlider.onValueChanged.AddListener(OnMouseSensChanged);
        mouseSensYSlider.onValueChanged.AddListener(OnMouseSensChanged);
        controllerSensXSlider.onValueChanged.AddListener(OnControllerSensChanged);
        controllerSensYSlider.onValueChanged.AddListener(OnControllerSensChanged);
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
}