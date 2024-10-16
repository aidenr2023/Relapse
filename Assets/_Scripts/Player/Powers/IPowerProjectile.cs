using UnityEngine;

public interface IPowerProjectile
{
    public void Shoot(
        IPower power, PlayerPowerManager powerManager, PowerToken pToken,
        Vector3 position, Vector3 forward
    );
}