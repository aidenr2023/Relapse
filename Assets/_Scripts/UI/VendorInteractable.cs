using UnityEngine;

public class VendorInteractable : MonoBehaviour, IInteractable
{
    public GameObject GameObject => gameObject;

    public bool IsInteractable { get; set; } = true;

    public bool HasOutline { get; set; }

    public void Interact(PlayerInteraction playerInteraction)
    {
        // Get the vendor menu instance & activate the shop
        VendorMenu.Instance.StartVendor();
        Debug.Log("Did a thing");

    }

    public void LookAtUpdate(PlayerInteraction playerInteraction)
    {
    }

    public string InteractText(PlayerInteraction playerInteraction)
    {
        return $"Open Shop";
    }
}