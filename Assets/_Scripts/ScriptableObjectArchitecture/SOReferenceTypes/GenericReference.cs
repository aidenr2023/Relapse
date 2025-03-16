using System;
using UnityEngine;

/// <summary>
/// Base class for all references to ScriptableObjects.
/// </summary>
/// <typeparam name="TVarType">The type of the constant variable.</typeparam>
/// <typeparam name="TSoType">The scriptable object type</typeparam>
[Serializable]
public abstract class GenericReference<TVarType, TSoType> where TSoType : GenericVariable<TVarType>
{
    [SerializeField] private bool useConstant = false;
    [SerializeField] private TVarType constantValue;
    [SerializeField] private TSoType variable;


    public TVarType Value
    {
        get => useConstant ? constantValue : variable.value;
        set
        {
            if (useConstant)
                constantValue = value;

            else
                variable.value = value;
        }
    }
}