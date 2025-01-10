using System;

public interface IGunHolder
{
    public Action<WeaponManager, IGun> OnGunEquipped { get; set; }
    public Action<WeaponManager, IGun> OnGunRemoved { get; set; }

    public IGun EquippedGun { get; }

    public void EquipGun(IGun gun);
    public void RemoveGun();
}