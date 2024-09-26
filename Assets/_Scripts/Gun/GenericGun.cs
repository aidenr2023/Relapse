using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GenericGun : MonoBehaviour, IGun
{
    [SerializeField] private GunInformation gunInformation;

    public GunInformation GunInformation => gunInformation;

    public Collider Collider { get; private set; }

    public GameObject GameObject => gameObject;

    private void Awake()
    {
        // Get the collider component
        Collider = GetComponent<Collider>();
    }

    public void Fire(WeaponManager weaponManager, Vector3 startingPosition, Vector3 direction)
    {
    }
}