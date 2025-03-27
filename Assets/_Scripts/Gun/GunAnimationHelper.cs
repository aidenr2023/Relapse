using UnityEngine;

public class GunAnimationHelper : MonoBehaviour
{
    [SerializeField] private GenericGun gun;

    public void OnReloadAnimationStart() => gun.OnReloadAnimationStart();
    
    public void OnReloadAnimationEnd() => gun.OnReloadAnimationEnd();
}