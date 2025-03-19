using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

public abstract class GenericVariable<T> : ValueVariable<T>
{
    [Header("AFTER PLAY MODE, THIS RESETS TO ITS ORIGINAL VALUE")] [SerializeField]
    public T value;

    [SerializeField] public T defaultValue;

    protected override void CustomReset()
    {
        value = defaultValue;
    }

    public override T GetValue()
    {
        return value;
    }

    public override void SetValue(T newValue)
    {
        value = newValue;
    }
}