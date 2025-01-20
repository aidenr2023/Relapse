using UnityEngine;

public class PlayerWeaponManagerHelper : MonoBehaviour
{
    public void ForceEquipGun(GameObject gunPrefab)
    {
        // Get the instance of the player weapon manager
        var weaponManager = Player.Instance.WeaponManager;

        // If the player weapon manager is null, return
        if (weaponManager == null)
            return;

        // Get the IGun component from the gun
        var gunComponent = gunPrefab.GetComponent<IGun>();

        // Return if the gun component is null
        if (gunComponent == null)
            return;

        // Instantiate the gun prefab
        var gun = Instantiate(gunPrefab, weaponManager.transform);

        // Get the IGun component from the instantiated gun
        var gunInstance = gun.GetComponent<IGun>();

        // Equip the gun
        weaponManager.EquipGun(gunInstance);
    }

    public void ForceDequipGun()
    {
        // Get the instance of the player weapon manager
        var weaponManager = Player.Instance.WeaponManager;

        // If the player weapon manager is null, return
        if (weaponManager == null)
            return;

        // Return if the weapon manager has no gun equipped
        if (weaponManager.EquippedGun == null)
            return;

        // Dequip the gun
        weaponManager.RemoveGun();
    }

    public void ForceRemoveGunFromGame()
    {
        // Get the instance of the player weapon manager
        var weaponManager = Player.Instance.WeaponManager;

        // If the player weapon manager is null, return
        if (weaponManager == null)
            return;

        // Return if the weapon manager has no gun equipped
        if (weaponManager.EquippedGun == null)
            return;

        // Get the equipped gun
        var equippedGun = weaponManager.EquippedGun;

        // Dequip the gun
        weaponManager.RemoveGun();

        // Destroy the equipped gun
        Destroy(equippedGun.GameObject);
    }
}