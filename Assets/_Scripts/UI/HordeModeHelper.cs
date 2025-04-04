using System.Collections;
using UnityEngine;

public class HordeModeHelper : MonoBehaviour
{
    [SerializeField] private HordeModeVendorInteractable vendor;
    [SerializeField, Min(0)] private float moveBackToVendorDelay = 3f;

    public void MovePlayerBackToVendor(Transform vendorPosition)
    {
        // Get the instance of the player
        var playerInstance = Player.Instance;

        // Check if the player instance is null
        if (playerInstance == null)
        {
            Debug.LogError("Player instance is null. Cannot move player to vendor.");
            return;
        }

        // Start the coroutine to move the player back to the vendor
        StartCoroutine(MovePlayerBackToVendorCoroutine(playerInstance, vendorPosition, moveBackToVendorDelay));
    }

    private IEnumerator MovePlayerBackToVendorCoroutine(Player playerInstance, Transform vendorPosition, float delay)
    {
        var startTime = Time.time;

        // Create a new tooltip saying how long until the player is moved
        JournalTooltipManager.Instance.AddTooltip(
            () => $"Returning to the vendor in {delay - (int)(Time.time - startTime + 1)} seconds",
            delay, true, () => Time.time - startTime >= delay
        );

        yield return new WaitForSeconds(delay);

        // Move the player to the vendor's position
        playerInstance.Rigidbody.MovePosition(vendorPosition.position);

        // Make the player face the direction of the vendor
        playerInstance.PlayerLook.ApplyRotation(vendorPosition.rotation);
    }

    public void ReinitializeVendor()
    {
        // Reinitialize the vendor
        vendor.Reinitialize();
    }
}