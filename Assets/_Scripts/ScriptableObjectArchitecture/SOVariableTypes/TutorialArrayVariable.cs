using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "Tutorial Array Variable",
    menuName = SOAHelper.ASSET_MENU_PATH + "Tutorial Array Variable"
)]
public class TutorialArrayVariable : GenericVariable<List<Tutorial>>
{
}