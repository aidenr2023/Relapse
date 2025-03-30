using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGun : IInteractable, IDamager
{
    public GunInformation GunInformation { get; }

    public GunModelType GunModelType { get; }

    public Collider Collider { get; }

    public float ReloadingPercentage { get; }

    public bool IsReloading { get; }

    public bool IsMagazineEmpty { get; }

    public int CurrentAmmo { get; set; }

    public bool IsReloadAnimationPlaying { get; }
    
    public bool IsFiring { get; }
    
    public Action<IGun> OnShoot { get; set; }
    
    public Action<IGun> OnReloadStart { get; set; }
    public Action<IGun> OnReloadStop { get; set; }

    public void OnFire(WeaponManager weaponManager);
    public void OnFireReleased();

    public void Fire(WeaponManager weaponManager, Vector3 startingPosition, Vector3 direction);
    public void Reload();

    public void OnEquipToPlayer(WeaponManager weaponManager);
    public void OnRemovalFromPlayer(WeaponManager weaponManager);
}