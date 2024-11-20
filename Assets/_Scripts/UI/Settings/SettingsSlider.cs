using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[ExecuteAlways]
public class SettingsSlider : MonoBehaviour
{
    [SerializeField] private string settingsText;

    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text settingsInfoText;
    [SerializeField] private TMP_InputField inputField;

    [SerializeField] public UnityEvent<float> onValueChanged;

    #region Getters

    public float Value
    {
        get => slider.value;
        set => slider.value = value;
    }

    #endregion

    private void Start()
    {
        // Connect the OnValueChanged event to the slider's onValueChanged event
        slider.onValueChanged.AddListener(OnSliderValueChanged);

        inputField.onEndEdit.AddListener(OnEditInputField);
    }

    private void OnSliderValueChanged(float value)
    {
        onValueChanged.Invoke(value);

        // Force the text to update
        SetText();
    }

    private void OnEditInputField(string value)
    {
        if (!float.TryParse(value, out var result))
            return;

        // Clamp the value
        result = Mathf.Clamp(result, slider.minValue, slider.maxValue);

        slider.value = result;

        // Force the text to update
        SetText();
    }

    private void Update()
    {
        if (settingsInfoText != null)
            settingsInfoText.text = settingsText;
    }

    private void SetText()
    {
        inputField.text = $"{slider.value:0.00}";
    }
}