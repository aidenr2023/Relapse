using UnityEngine;

[CreateAssetMenu(
    fileName = "Power Array Variable",
    menuName = SOAHelper.ASSET_MENU_PATH + SOAHelper.ADVANCED + "Power Array Variable"
)]
public class PowerArrayVariable : GenericVariable<PowerScriptableObject[]>
{
}