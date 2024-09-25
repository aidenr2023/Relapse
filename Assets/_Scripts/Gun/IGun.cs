using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGun
{
    public GunInformation GunInformation { get; }

    public Collider Collider { get; }

    /// <summary>
    /// A reference to the game object that the gun script is attached to.
    /// </summary>
    public GameObject GameObject { get; }

    public void Fire(WeaponManager weaponManager, Vector3 startingPosition, Vector3 direction);
}