using System;
using UnityEngine;


/// <summary>
/// A class that contains the indentifier information for variables that will be used to save / load data.
/// </summary>
[Serializable]
[CreateAssetMenu(fileName = "New Serialization Data Info", menuName = "Serialization/Serialization Data Info")]
public class SerializationDataInfo : ScriptableObject
{
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
}