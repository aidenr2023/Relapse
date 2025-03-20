using System;
using UnityEngine.Events;

[Serializable]
public class EventReference : GenericReference<UnityEvent, EventVariable>
{
    public static EventReference operator +(
        EventReference healthChangedEventVariable,
        UnityAction action
    )
    {
        healthChangedEventVariable.Value.AddListener(action);
        return healthChangedEventVariable;
    }

    public static EventReference operator -(
        EventReference healthChangedEventVariable,
        UnityAction action
    )
    {
        healthChangedEventVariable.Value.RemoveListener(action);
        return healthChangedEventVariable;
    }
}