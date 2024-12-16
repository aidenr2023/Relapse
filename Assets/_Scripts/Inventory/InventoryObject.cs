using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Inventory Object", menuName = "Inventory Object")]
public class InventoryObject : ScriptableObject
{
    [FormerlySerializedAs("name")] [SerializeField] private string itemName;

    public string ItemName => itemName;
}