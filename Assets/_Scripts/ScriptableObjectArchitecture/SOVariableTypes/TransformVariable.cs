using UnityEngine;

[CreateAssetMenu(
    fileName = "Transform Variable",
    menuName = SOAHelper.ASSET_MENU_PATH + SOAHelper.BASIC + "Transform Variable"
)]
public class TransformVariable : GenericVariable<Transform>
{
}