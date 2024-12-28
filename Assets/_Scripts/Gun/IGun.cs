using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGun : IInteractable, IDamager
{
    public GunInformation GunInformation { get; }

    public Collider Collider { get; }

    public float ReloadingPercentage { get; }

    public bool IsReloading { get; }

    public bool IsMagazineEmpty { get; }

    public int CurrentAmmo { get; set; }

    public void OnFire(WeaponManager weaponManager);
    public void OnFireReleased();

    public void Fire(WeaponManager weaponManager, Vector3 startingPosition, Vector3 direction);
    public void Reload();

    public void OnEquip(WeaponManager weaponManager);
    public void OnRemoval(WeaponManager weaponManager);

    public void UpdateOutline(WeaponManager weaponManager);
}