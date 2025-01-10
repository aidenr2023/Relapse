using UnityEngine;

public interface IDataInfo
{
    public string VariableName { get; }

    public SerializationDataType DataType { get; }

    public bool GetBoolValue();

    public double GetNumberValue();

    public string GetStringValue();

    public Vector3 GetVector3Value();
}