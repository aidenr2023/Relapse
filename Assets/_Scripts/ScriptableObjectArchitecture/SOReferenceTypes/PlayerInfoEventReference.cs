using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class PlayerInfoEventReference : GenericReference<UnityEvent<PlayerInfo>, PlayerInfoEventVariable>
{
    public static PlayerInfoEventReference operator +(
        PlayerInfoEventReference playerInfoEventVariable,
        UnityAction<PlayerInfo> action
    )
    {
        playerInfoEventVariable.Value.AddListener(action);
        return playerInfoEventVariable;
    }

    public static PlayerInfoEventReference operator -(
        PlayerInfoEventReference playerInfoEventVariable,
        UnityAction<PlayerInfo> action
    )
    {
        playerInfoEventVariable.Value.RemoveListener(action);
        return playerInfoEventVariable;
    }
}