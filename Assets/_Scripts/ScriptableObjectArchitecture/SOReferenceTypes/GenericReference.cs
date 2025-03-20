using System;
using UnityEngine;

/// <summary>
/// Base class for all references to ScriptableObjects.
/// </summary>
/// <typeparam name="TVarType">The type of the constant variable.</typeparam>
/// <typeparam name="TSoType">The scriptable object type</typeparam>
[Serializable]
public abstract class GenericReference<TVarType, TSoType>
    where TSoType : ValueVariable<TVarType>
{
    [SerializeField] protected bool useConstant = true;
    [SerializeField] protected TVarType constantValue;
    [SerializeField] protected TSoType variable;

    /// <summary>
    /// Forces the reference to use the constant value.
    /// </summary>
    public void ForceUseConstant()
    {
        useConstant = true;
    }

    public TVarType Value
    {
        get
        {
            if (useConstant)
                return constantValue;

            if (variable == null)
                return default(TVarType);

            return variable.GetValue();
        }
        set
        {
            if (useConstant)
                constantValue = value;

            else
                variable.SetValue(value);
        }
    }
}