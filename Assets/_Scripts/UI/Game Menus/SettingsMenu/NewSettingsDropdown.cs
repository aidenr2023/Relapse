using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class NewSettingsDropdown : MonoBehaviour, IResetableSettingElement
{
    [Space, SerializeField] private TMP_Dropdown dropdown;
    [Space, SerializeField] private SettingsHelper settingsHelper;
    [SerializeField] private DropdownSettingType settingType;

    private UnityEvent<NewSettingsDropdown, DropdownSettingType, int> _onValueChanged;

    public TMP_Dropdown Dropdown => dropdown;
    public DropdownSettingType SettingType => settingType;

    private void Awake()
    {
        _onValueChanged = new UnityEvent<NewSettingsDropdown, DropdownSettingType, int>();
        _onValueChanged.AddListener(settingsHelper.SetSettingValue);
        
        dropdown.onValueChanged.AddListener(InvokeOnValueChanged);
    }

    private void InvokeOnValueChanged(int index)
    {
        // Invoke the event with the current dropdown value
        _onValueChanged.Invoke(this, settingType, index);
    }

    private void Start()
    {
        // Initialize the dropdown options
        settingsHelper.InitializeDropdownOptions(this);
    }

    private void OnEnable()
    {
        settingsHelper.OnSettingChangedDropdown += ForceValueOnSettingChanged;

        // Subscribe to the reset event of the settingsHelper
        settingsHelper.OnReset += ResetToSettingOnReset;
    }

    private void OnDisable()
    {
        settingsHelper.OnSettingChangedDropdown -= ForceValueOnSettingChanged;

        // Subscribe to the reset event of the settingsHelper
        settingsHelper.OnReset -= ResetToSettingOnReset;
    }

    public bool TryGetOptionValue<T>(int index, out T value)
    {
        value = default;
        
        // If the index is out of range, return false
        if (index < 0 || index >= dropdown.options.Count)
        {
            Debug.LogError($"Index {index} is out of range for the dropdown options. Returning default value.");
            return false;
        }
        
        // Get the option data at the specified index and cast it to CustomOptionData
        var customOptionData = dropdown.options[index] as CustomOptionData;
        
        // If the cast fails, return false
        if (customOptionData == null)
        {
            Debug.LogError($"Failed to cast option data at index {index} to CustomOptionData. Returning default value.");
            return false;
        }
        
        // Try to get the value from the CustomOptionData
        if (!customOptionData.TryGetValue(out T castedValue))
        {
            Debug.LogError($"Failed to get value from CustomOptionData at index {index}. Returning default value.");
            return false;
        }
        
        // If the cast is successful, set the value and return true
        value = castedValue;
        
        Debug.Log($"Successfully casted option value at index {index} to type {typeof(T)} -> {value.ToString()}.");

        return true;
    }

    private void ForceValueOnSettingChanged(DropdownSettingType type, object value)
    {
        // Return if the type is not the same as the setting type
        if (type != settingType)
            return;

        // // dropdown.options.Select(n => n.).IndexOf(value);
        //
        // // Set the slider value to the new value
        // dropdown.value = value;
    }

    public void ResetToSetting()
    {
    }

    private void ResetToSettingOnReset(SettingsHelper _) => ResetToSetting();


}