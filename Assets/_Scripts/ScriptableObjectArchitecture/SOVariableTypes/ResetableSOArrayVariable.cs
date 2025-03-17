using UnityEngine;

[CreateAssetMenu(
    fileName = "Resetable SO Array Variable",
    menuName = SOAHelper.ASSET_MENU_PATH + SOAHelper.ADVANCED + "Resetable SO Array Variable"
)]
public class ResetableSOArrayVariable : GenericVariable<ResetableScriptableObject[]>
{
    protected override void CustomReset()
    {
        base.CustomReset();

        // Return if value is null
        if (defaultValue == null)
            return;

        // Reset each element in the array
        foreach (var variable in defaultValue)
            variable?.Reset();
    }
}