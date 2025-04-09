using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody))]
public class Shotgun : GenericGun
{
    private static readonly int ShootingAnimationID = Animator.StringToHash("Shooting");

    #region Serialized Fields

    [SerializeField] private GameObject displayModelObject;
    [SerializeField] private GameObject weaponModelObject;

    [SerializeField] private ParticleSystem muzzleParticles;
    [SerializeField] [Range(0, 500)] private int muzzleParticlesCount = 200;

    [SerializeField] private int pelletsPerShot = 8;

    #endregion

    public override void Fire(WeaponManager weaponManager, Vector3 startingPosition, Vector3 direction)
    {
        // If the gun is not firing, return
        if (!ShouldFire())
            return;

        // Determine how many times the gun should fire this frame
        var timesToFire = (int)(fireDelta / TimeBetweenShots);
        fireDelta %= TimeBetweenShots;

        // Overfill the mag before firing
        currentMagazineSize += (pelletsPerShot - 1) * timesToFire;

        // // TODO: Delete eventually when the shotgun has the new VFX
        // // Emit the fire particles
        // PlayParticles(muzzleParticles, muzzleLocation.position, muzzleParticlesCount);

        // Fire the gun
        ShootProjectiles(weaponManager, pelletsPerShot, startingPosition, direction);

        currentMagazineSize = gunInformation.MagazineSize;
    }

    public override bool Reload()
    {
        return false;

        return base.Reload();
    }

    public void SetDisplayModelObject(bool displayOrWeapon)
    {
        if (displayModelObject == null || weaponModelObject == null)
            return;
        
        if (displayOrWeapon)
        {
            displayModelObject.SetActive(true);
            weaponModelObject.SetActive(false);
        }
        else
        {
            displayModelObject.SetActive(false);
            weaponModelObject.SetActive(true);
        }
        
        Debug.Log($"Set display model object to {displayOrWeapon}", this);
    }
}