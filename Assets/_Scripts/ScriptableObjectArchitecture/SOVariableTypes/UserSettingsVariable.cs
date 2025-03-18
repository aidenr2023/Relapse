using UnityEngine;

[CreateAssetMenu(
    fileName = "User Settings Variable",
    menuName = SOAHelper.ASSET_MENU_PATH + SOAHelper.SETTINGS + "User Settings Variable"
)]
public class UserSettingsVariable : GenericVariable<UserSettings>
{
}