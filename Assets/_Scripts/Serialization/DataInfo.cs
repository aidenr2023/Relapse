using UnityEngine;

public readonly struct DataInfo : IDataInfo
{
    public string VariableName { get; }
    public SerializationDataType DataType { get; }
    private readonly bool _boolValue;
    private readonly double _numberValue;
    private readonly string _stringValue;
    private readonly Vector3 _vector3Value;

    public DataInfo(string variableName, bool boolValue)
    {
        VariableName = variableName;
        DataType = SerializationDataType.Boolean;

        _boolValue = boolValue;
        _numberValue = default;
        _stringValue = default;
        _vector3Value = default;
    }

    public DataInfo(string variableName, double numberValue)
    {
        VariableName = variableName;
        DataType = SerializationDataType.Number;

        _boolValue = default;
        _numberValue = numberValue;
        _stringValue = default;
        _vector3Value = default;
    }

    public DataInfo(string variableName, string stringValue)
    {
        VariableName = variableName;
        DataType = SerializationDataType.String;

        _boolValue = default;
        _numberValue = default;
        _stringValue = stringValue;
        _vector3Value = default;
    }

    public DataInfo(string variableName, Vector3 vector3Value)
    {
        VariableName = variableName;
        DataType = SerializationDataType.Vector3;

        _boolValue = default;
        _numberValue = default;
        _stringValue = default;
        _vector3Value = vector3Value;
    }

    public bool GetBoolValue()
    {
        return _boolValue;
    }

    public double GetNumberValue()
    {
        return _numberValue;
    }

    public string GetStringValue()
    {
        return _stringValue;
    }

    public Vector3 GetVector3Value()
    {
        return _vector3Value;
    }
}