using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(
    fileName = "Gun Event Variable",
    menuName = SOAHelper.ASSET_MENU_PATH + SOAHelper.EVENT + "Gun Event Variable"
)]
public class GunEventVariable : GenericEventVariable<IGun>
{
    public static GunEventVariable operator +(
        GunEventVariable gunEventVariable,
        UnityAction<IGun> action
    )
    {
        gunEventVariable.value.AddListener(action);
        return gunEventVariable;
    }

    public static GunEventVariable operator -(
        GunEventVariable gunEventVariable,
        UnityAction<IGun> action
    )
    {
        gunEventVariable.value.RemoveListener(action);
        return gunEventVariable;
    }
}