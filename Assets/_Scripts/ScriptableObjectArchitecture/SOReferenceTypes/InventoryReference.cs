using System;
using System.Collections.Generic;

[Serializable]
public class InventoryReference : GenericReference<List<InventoryEntry>, InventoryVariable>
{
}