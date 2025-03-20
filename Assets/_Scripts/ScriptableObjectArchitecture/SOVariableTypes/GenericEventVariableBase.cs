using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class GenericEventVariableBase<TEventType> : ValueVariable<TEventType>
    where TEventType : UnityEventBase
{
    [Header("THIS VALUE CAN ONLY BE UPDATED IN PLAY MODE"), SerializeField]
    protected TEventType value;

    public TEventType Value => value;

    protected override void CustomReset()
    {
        base.CustomReset();

        // Return if the value is null
        if (value == null) 
            return;
        
        // Clear all NON-PERSISTENT listeners from the event
        value.RemoveAllListeners();
    }

    public override TEventType GetValue()
    {
        return value;
    }

    public override void SetValue(TEventType newValue)
    {
        Debug.LogError("You probably shouldn't be setting the value of an event variable directly.");

        value = newValue;
    }
    
    public void LogString(string text)
    {
        Debug.Log(text);
    }
}

public abstract class GenericEventVariable<T1> : GenericEventVariableBase<UnityEvent<T1>>
{
    public void Invoke(T1 arg1)
    {
        value.Invoke(arg1);
    }
}

public abstract class GenericEventVariable<T1, T2> : GenericEventVariableBase<UnityEvent<T1, T2>>
{
    public void Invoke(T1 arg1, T2 arg2)
    {
        value.Invoke(arg1, arg2);
    }
}

public abstract class GenericEventVariable<T1, T2, T3> : GenericEventVariableBase<UnityEvent<T1, T2, T3>>
{
    public void Invoke(T1 arg1, T2 arg2, T3 arg3)
    {
        value.Invoke(arg1, arg2, arg3);
    }
}

public abstract class GenericEventVariable<T1, T2, T3, T4> : GenericEventVariableBase<UnityEvent<T1, T2, T3, T4>>
{
    public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        value.Invoke(arg1, arg2, arg3, arg4);
    }
}