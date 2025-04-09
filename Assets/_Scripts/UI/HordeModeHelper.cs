using System.Collections;
using UnityEngine;

public class HordeModeHelper : MonoBehaviour
{
    public void MovePlayerBackToVendor()
    {
        // Run the move player back to vendor function from the instance
        var result = HordeModeManager.Instance
            .Match(instance => instance.MovePlayerBackToVendor());
    }


    public void ReinitializeVendor()
    {
        // Run the initialize vendor function from the instance
        HordeModeManager.Instance
            .Match(instance => instance.ReinitializeVendor());
    }
}