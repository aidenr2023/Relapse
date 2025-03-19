using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class HealthChangedEventReference : GenericReference<UnityEvent<object, HealthChangedEventArgs>,
    HealthChangedEventVariable>
{
}