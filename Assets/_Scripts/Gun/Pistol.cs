using UnityEngine;

public class Pistol : MonoBehaviour, IGun
{
    [SerializeField] private GunInformation gunInformation;

    public GunInformation GunInformation => gunInformation;

    public GameObject GameObject => gameObject;

    public void Fire(WeaponManager weaponManager, Vector3 startingPosition, Vector3 direction)
    {
    }
}