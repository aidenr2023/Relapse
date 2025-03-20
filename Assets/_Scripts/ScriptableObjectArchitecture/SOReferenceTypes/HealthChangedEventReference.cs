using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class HealthChangedEventReference : GenericReference<UnityEvent<object, HealthChangedEventArgs>,
    HealthChangedEventVariable>
{
    public HealthChangedEventReference()
    {
        constantValue = new UnityEvent<object, HealthChangedEventArgs>();
    }
    
    public static HealthChangedEventReference operator +(
        HealthChangedEventReference healthChangedEventVariable,
        UnityAction<object, HealthChangedEventArgs> action
    )
    {
        healthChangedEventVariable.Value.AddListener(action);
        return healthChangedEventVariable;
    }

    public static HealthChangedEventReference operator -(
        HealthChangedEventReference healthChangedEventVariable,
        UnityAction<object, HealthChangedEventArgs> action
    )
    {
        healthChangedEventVariable.Value.RemoveListener(action);
        return healthChangedEventVariable;
    }
}