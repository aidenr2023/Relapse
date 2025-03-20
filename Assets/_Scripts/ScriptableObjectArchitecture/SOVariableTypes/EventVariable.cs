
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(
    fileName = "Event Variable",
    menuName = SOAHelper.ASSET_MENU_PATH + SOAHelper.EVENT + "Event Variable"
)]
public class EventVariable : GenericEventVariable
{
    public static EventVariable operator +(
        EventVariable healthChangedEventVariable,
        UnityAction action
    )
    {
        healthChangedEventVariable.value.AddListener(action);
        return healthChangedEventVariable;
    }

    public static EventVariable operator -(
        EventVariable healthChangedEventVariable,
        UnityAction action
    )
    {
        healthChangedEventVariable.value.RemoveListener(action);
        return healthChangedEventVariable;
    }
}