using UnityEngine;

[CreateAssetMenu(
    fileName = "FloatVariable",
    menuName = SOAHelper.ASSET_MENU_PATH + "Power Array Variable"
)]
public class PowerArrayVariable : GenericVariable<PowerScriptableObject[]>
{
}