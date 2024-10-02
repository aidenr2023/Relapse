using UnityEngine;

public interface IPowerProjectile
{
    public void Shoot(
        IPower power, TestPlayerPowerManager powerManager, PowerToken pToken,
        Vector3 position, Vector3 forward
    );
}