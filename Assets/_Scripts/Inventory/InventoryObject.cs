using UnityEngine;

[CreateAssetMenu(fileName = "New Inventory Object", menuName = "Inventory Object")]
public class InventoryObject : ScriptableObject
{
    [SerializeField] private string name;

    public string Name => name;
}