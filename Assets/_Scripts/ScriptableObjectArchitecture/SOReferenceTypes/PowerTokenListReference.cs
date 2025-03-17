using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class PowerTokenListReference : GenericReference<List<PowerToken>, PowerTokenListVariable>
{
    public PowerToken GetPowerToken(PowerScriptableObject powerScriptableObject)
    {
        return Value.FirstOrDefault(powerToken => powerToken.PowerScriptableObject == powerScriptableObject);
    }
}