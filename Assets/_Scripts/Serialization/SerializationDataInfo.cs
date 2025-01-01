using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A class that contains the indentifier information for variables that will be used to save / load data.
/// </summary>
[Serializable]
[CreateAssetMenu(fileName = "New Serialization Data Info", menuName = "Serialization/Serialization Data Info")]
public class SerializationDataInfo : ScriptableObject
{
    // A dictionary that contains the data info for each variable
    private static readonly Dictionary<string, SerializationDataInfo> _dataInfoDictionary = new();

    public static IReadOnlyDictionary<string, SerializationDataInfo> DataInfoDictionary => _dataInfoDictionary;

    [SerializeField] public SerializationDataType dataType;
    // [SerializeField] private string variableName;

    [SerializeField] private bool boolValue;
    [SerializeField] private float numberValue;
    [SerializeField] private string stringValue;

    #region Getters

    public SerializationDataType DataType => dataType;

    // public string VariableName => variableName;
    public string VariableName => name;

    #endregion

    private void Awake()
    {
    }

    private void OnEnable()
    {
        // Add this instance to the dictionary
        if (!_dataInfoDictionary.TryAdd(name, this))
            _dataInfoDictionary[name] = this;
    }

    public void SetBoolValue(bool value)
    {
        boolValue = value;
    }

    public void SetNumberValue(float value)
    {
        numberValue = value;
    }

    public void SetStringValue(string value)
    {
        stringValue = value;
    }

    public void SetValue<T>(SerializationDataType type, T data) where T : struct
    {
        var dataAsObject = data as object;

        switch (type)
        {
            case SerializationDataType.Boolean:
                SetBoolValue((bool)dataAsObject);
                break;

            case SerializationDataType.Number:
                SetNumberValue((float)dataAsObject);
                break;

            case SerializationDataType.String:
                SetStringValue((string)dataAsObject);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    public void SetDataType(SerializationDataType type)
    {
        dataType = type;
    }

    public bool GetBoolValue()
    {
        return boolValue;
    }

    public float GetNumberValue()
    {
        return numberValue;
    }

    public string GetStringValue()
    {
        return stringValue;
    }

    #region Factory Methods

    public static SerializationDataInfo SetOrCreateNumberData(string dataName, float value)
    {
        // Check if the instance of the variable already exists
        if (!_dataInfoDictionary.TryGetValue(dataName, out var data))
        {
            // Create a new instance of the SerializationDataInfo class
            data = CreateInstance<SerializationDataInfo>();

            // Remove the item from the dictionary
            _dataInfoDictionary.Remove(data.name);

            data.name = dataName;

            // Add the item back to the dictionary
            _dataInfoDictionary.Add(dataName, data);
        }

        // Set the value of the data
        data.SetDataType(SerializationDataType.Number);
        data.SetNumberValue(value);

        return data;
    }

    public static SerializationDataInfo SetOrCreateBooleanData(string dataName, bool value)
    {
        // Check if the instance of the variable already exists
        if (!_dataInfoDictionary.TryGetValue(dataName, out var data))
        {
            // Create a new instance of the SerializationDataInfo class
            data = CreateInstance<SerializationDataInfo>();

            // Remove the item from the dictionary
            _dataInfoDictionary.Remove(data.name);

            data.name = dataName;

            // Add the item back to the dictionary
            _dataInfoDictionary.Add(dataName, data);
        }

        // Set the value of the data
        data.SetDataType(SerializationDataType.Boolean);
        data.SetBoolValue(value);

        return data;
    }

    public static SerializationDataInfo SetOrCreateStringData(string dataName, string value)
    {
        // Check if the instance of the variable already exists
        if (!_dataInfoDictionary.TryGetValue(dataName, out var data))
        {
            // Create a new instance of the SerializationDataInfo class
            data = CreateInstance<SerializationDataInfo>();

            // Remove the item from the dictionary
            _dataInfoDictionary.Remove(data.name);

            data.name = dataName;

            // Add the item back to the dictionary
            _dataInfoDictionary.Add(dataName, data);
        }

        // Set the value of the data
        data.SetDataType(SerializationDataType.String);
        data.SetStringValue(value);

        return data;
    }

    #endregion
}