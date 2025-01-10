using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A class that contains the indentifier information for variables that will be used to save / load data.
/// </summary>
[Serializable]
[CreateAssetMenu(fileName = "New Serialization Data Info", menuName = "Serialization/Serialization Data Info")]
public class SerializationDataInfo : ScriptableObject, IDataInfo
{
    // A dictionary that contains the data info for each variable
    private static readonly Dictionary<string, SerializationDataInfo> _dataInfoDictionary = new();

    public static IReadOnlyDictionary<string, SerializationDataInfo> DataInfoDictionary => _dataInfoDictionary;

    [SerializeField] public SerializationDataType dataType;
    // [SerializeField] private string variableName;

    [SerializeField] private bool boolValue;
    [SerializeField] private double numberValue;
    [SerializeField] private string stringValue;
    [SerializeField] private Vector3 vector3Value;

    #region Getters

    public SerializationDataType DataType => dataType;

    // public string VariableName => variableName;
    public string VariableName => name;

    #endregion

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

    public void SetNumberValue(double value)
    {
        numberValue = value;
    }

    public void SetStringValue(string value)
    {
        stringValue = value;
    }

    public void SetVector3Value(Vector3 value)
    {
        vector3Value = value;
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

            case SerializationDataType.Vector3:
                SetVector3Value((Vector3)dataAsObject);
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

    public double GetNumberValue()
    {
        return numberValue;
    }

    public string GetStringValue()
    {
        return stringValue;
    }

    public Vector3 GetVector3Value()
    {
        return vector3Value;
    }

    #region Factory Methods

    private static SerializationDataInfo SetNumberData(string dataName, float value)
    {
        // Check if the instance of the variable already exists
        if (!_dataInfoDictionary.TryGetValue(dataName, out var data))
            throw new KeyNotFoundException($"The data with the name {dataName} does not exist in the dictionary!");

        // Set the value of the data
        data.SetDataType(SerializationDataType.Number);
        data.SetNumberValue(value);

        return data;
    }

    private static SerializationDataInfo SetBooleanData(string dataName, bool value)
    {
        // Check if the instance of the variable already exists
        if (!_dataInfoDictionary.TryGetValue(dataName, out var data))
            throw new KeyNotFoundException($"The data with the name {dataName} does not exist in the dictionary!");

        // Set the value of the data
        data.SetDataType(SerializationDataType.Boolean);
        data.SetBoolValue(value);

        return data;
    }

    private static SerializationDataInfo SetStringData(string dataName, string value)
    {
        // Check if the instance of the variable already exists
        if (!_dataInfoDictionary.TryGetValue(dataName, out var data))
            throw new KeyNotFoundException($"The data with the name {dataName} does not exist in the dictionary!");

        // Set the value of the data
        data.SetDataType(SerializationDataType.String);
        data.SetStringValue(value);

        return data;
    }

    #endregion
}