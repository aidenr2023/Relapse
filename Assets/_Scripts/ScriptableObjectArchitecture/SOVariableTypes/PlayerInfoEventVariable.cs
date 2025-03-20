using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(
    fileName = "PlayerInfo Event Variable",
    menuName = SOAHelper.ASSET_MENU_PATH + SOAHelper.EVENT + "PlayerInfo Event Variable"
)]
public class PlayerInfoEventVariable : GenericEventVariable<PlayerInfo>
{
    public static PlayerInfoEventVariable operator +(
        PlayerInfoEventVariable playerInfoEventVariable,
        UnityAction<PlayerInfo> action
    )
    {
        playerInfoEventVariable.value.AddListener(action);
        return playerInfoEventVariable;
    }

    public static PlayerInfoEventVariable operator -(
        PlayerInfoEventVariable playerInfoEventVariable,
        UnityAction<PlayerInfo> action
    )
    {
        playerInfoEventVariable.value.RemoveListener(action);
        return playerInfoEventVariable;
    }
}