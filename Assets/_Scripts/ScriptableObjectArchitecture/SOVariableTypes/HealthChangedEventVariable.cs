using System;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(
    fileName = "Health Changed Event Variable",
    menuName = SOAHelper.ASSET_MENU_PATH + SOAHelper.EVENT + "Health Changed Event Variable"
)]
public class HealthChangedEventVariable : GenericEventVariable<object, HealthChangedEventArgs>
{
    public void LogHealthEvent(object sender, HealthChangedEventArgs e)
    {
        Debug.Log($"Player health changed: {e.Amount:0.00}!");
    }

    public static HealthChangedEventVariable operator +(
        HealthChangedEventVariable healthChangedEventVariable,
        UnityAction<object, HealthChangedEventArgs> action
    )
    {
        healthChangedEventVariable.value.AddListener(action);
        return healthChangedEventVariable;
    }

    public static HealthChangedEventVariable operator -(
        HealthChangedEventVariable healthChangedEventVariable,
        UnityAction<object, HealthChangedEventArgs> action
    )
    {
        healthChangedEventVariable.value.RemoveListener(action);
        return healthChangedEventVariable;
    }
}