using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

/// <summary>
/// This class takes in the data that was serialized.
/// It then reads this data and checks if it meets a specific condition based on the type of data.
/// </summary>
[Serializable]
public class SerializationDataEvent
{
    [SerializeField] private SerializationDataInfo dataInfo;

    [SerializeField] private bool boolConditionalValue;
    [SerializeField] private float numberConditionalValue;
    [SerializeField] private string stringConditionalValue;

    [SerializeField] private UnityEvent onFalse;
    [SerializeField] private UnityEvent onTrue;

    public SerializationDataInfo DataInfo => dataInfo;

    public bool BoolConditionalValue => boolConditionalValue;
    public float NumberConditionalValue => numberConditionalValue;
    public string StringConditionalValue => stringConditionalValue;

    public UnityEvent OnFalse => onFalse;
    public UnityEvent OnTrue => onTrue;
}