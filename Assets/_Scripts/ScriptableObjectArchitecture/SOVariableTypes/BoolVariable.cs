﻿using UnityEngine;

[CreateAssetMenu(
    fileName = "Bool Variable",
    menuName = SOAHelper.ASSET_MENU_PATH + SOAHelper.BASIC + "Bool Variable"
)]
public class BoolVariable : GenericVariable<bool>
{
    public static implicit operator bool(BoolVariable variable) => variable.value;
}