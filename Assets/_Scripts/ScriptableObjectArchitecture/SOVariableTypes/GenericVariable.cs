using System;
using UnityEditor;
using UnityEngine;

public abstract class GenericVariable<T> : ResetableScriptableObject
{
    [Header("AFTER PLAY MODE, THIS RESETS TO ITS ORIGINAL VALUE")] [SerializeField]
    public T value;

    [SerializeField] public T defaultValue;

    protected override void CustomReset()
    {
        value = defaultValue;
    }
}