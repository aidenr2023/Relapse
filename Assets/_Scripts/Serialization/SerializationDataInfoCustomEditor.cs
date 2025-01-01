using System;
using UnityEditor;
using UnityEngine;

// Create a custom editor for the SerializationDataInfo scriptable object class.
// This class will only show the type of the data and the single corresponding value.

[CustomEditor(typeof(SerializationDataInfo))]
public class SerializationDataInfoCustomEditor : Editor
{
    private SerializationDataInfo _dataInfo;

    private void OnEnable()
    {
        _dataInfo = (SerializationDataInfo)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Create a dropdown to select the data type.
        // _dataInfo.DataType = (SerializationDataType)EditorGUILayout.EnumPopup("Data Type", _dataInfo.DataType);
        _dataInfo.SetDataType((SerializationDataType)EditorGUILayout.EnumPopup("Data Type", _dataInfo.DataType));

        // Write a message to the user to let them know that values they write here are NOT meant to be saved.
        EditorGUILayout.HelpBox(
            "The \"value\" variable is for testing purposes only. It will most likely be overwritten in-game when saving data!",
            MessageType.Warning
        );

        switch (_dataInfo.DataType)
        {
            case SerializationDataType.Boolean:
                _dataInfo.SetBoolValue(EditorGUILayout.Toggle("Value", _dataInfo.GetBoolValue()));
                break;

            case SerializationDataType.Number:
                _dataInfo.SetNumberValue(EditorGUILayout.FloatField("Value", _dataInfo.GetNumberValue()));
                break;

            case SerializationDataType.String:
                _dataInfo.SetStringValue(EditorGUILayout.TextField("Value", _dataInfo.GetStringValue()));
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        serializedObject.ApplyModifiedProperties();
    }
}