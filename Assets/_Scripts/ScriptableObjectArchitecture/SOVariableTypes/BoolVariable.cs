using UnityEngine;

[CreateAssetMenu(
    fileName = "Bool Variable",
    menuName = SOAHelper.ASSET_MENU_PATH + SOAHelper.BASIC + "Bool Variable"
)]
public class BoolVariable : GenericVariable<bool>
{
    public void SetValueInInspector(bool newValue)
    {
        value = newValue;
    }

    public void DestroyGameObject(GameObject gameObject)
    {
        if (gameObject != null)
            Destroy(gameObject);
    }

    public static implicit operator bool(BoolVariable variable) => variable.value;
}