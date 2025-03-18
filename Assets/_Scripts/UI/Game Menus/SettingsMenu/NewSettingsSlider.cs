using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class NewSettingsSlider : MonoBehaviour
{
    [SerializeField] private UserSettingsVariable settingsMenuSettings;

    [Space, SerializeField] private Slider slider;

    [Space, SerializeField] private SettingsHelper settingsHelper;
    [SerializeField] private SliderSettingType settingType;

    private UnityEvent<float, SliderSettingType> _onValueChanged;

    private void Awake()
    {
        _onValueChanged = new UnityEvent<float, SliderSettingType>();
        _onValueChanged.AddListener(settingsHelper.SetSettingValue);
    }

    private void ForceValueOnSettingChanged(SliderSettingType type, float value)
    {
        // Return if the type is not the same as the setting type
        if (type != settingType)
            return;

        // Set the slider value to the new value
        slider.value = value;
    }

    private void Start()
    {
        // Connect the OnValueChanged event to the slider's onValueChanged event
        slider.onValueChanged.AddListener(InvokeOnValueChanged);
    }

    private void OnEnable()
    {
        settingsHelper.OnSettingChanged += ForceValueOnSettingChanged;

        // Set the slider to the current setting value
        slider.value = SettingsHelper.GetSettingValue(settingType, settingsMenuSettings);
    }

    private void OnDisable()
    {
        settingsHelper.OnSettingChanged -= ForceValueOnSettingChanged;
    }

    private void InvokeOnValueChanged(float value)
    {
        _onValueChanged.Invoke(value, settingType);
    }
}