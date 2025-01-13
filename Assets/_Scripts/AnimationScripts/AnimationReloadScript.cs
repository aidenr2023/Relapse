using System;
using UnityEngine;

public class AnimationReloadScript : MonoBehaviour
{
    [SerializeField] private WeaponManager weaponManager;
    [SerializeField] private Animator playerAnimator;

    private void Start()
    {
        weaponManager.OnGunEquipped += OnGunEquipped;
        weaponManager.OnGunRemoved += OnGunRemoved;
    }

    private void OnGunEquipped(WeaponManager manager, IGun gun)
    {
        // Connect to the proper events
        gun.OnReloadStart += OnReload;
    }

    private void OnGunRemoved(WeaponManager weaponManager, IGun gun)
    {
        // Disconnect the proper events
        if (gun is not GenericGun genericGun)
            return;

        genericGun.OnReloadStart -= OnReload;
    }


    private void OnReload(IGun gun)
    {
        Debug.Log($"RELOAD EVENT IN THE OTHER SCRIPT: {gun.GunInformation}");

        // Check if the gun is a genericGun
        if (gun is not GenericGun genericGun)
            return;

        // Return if the player animator is null
        if (playerAnimator == null)
            return;

        // Set the player animator's trigger to reload
        playerAnimator.SetTrigger("Reload");

        // // Get the gun's animator
        // var gunAnimator = genericGun.Animator;
        //
        // // Return if the gun's animator is null
        // if (gunAnimator == null)
        //     return;
        //
        //
    }
}