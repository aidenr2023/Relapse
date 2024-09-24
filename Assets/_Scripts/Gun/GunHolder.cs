using UnityEngine;

public class GunHolder : MonoBehaviour
{
    
    private IGun _currentGun;
    
    public IGun CurrentGun => _currentGun;

    public void EquipGun(IGun gun)
    {
        // Dequip the current gun
        Dequip();
        
        // Set the current gun to the gun that was passed in
        _currentGun = gun;
    }

    public void Dequip()
    {
        // Set the current gun to null
        if (_currentGun == null)
            return;
        
        _currentGun = null;
    }
}