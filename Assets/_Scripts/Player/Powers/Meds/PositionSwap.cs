using UnityEngine;

public class PositionSwap : MonoBehaviour, IPower
{
    // a field for a projectile prefab
    [SerializeField] private GameObject projectilePrefab;

    [SerializeField] private LayerMask layersToHit;

    public GameObject GameObject => gameObject;
    public PowerScriptableObject PowerScriptableObject { get; set; }

    public Sound NormalHitSfx => PowerScriptableObject.NormalHitSfx;
    public Sound CriticalHitSfx => PowerScriptableObject.CriticalHitSfx;

    public string PassiveEffectDebugText(PlayerPowerManager powerManager, PowerToken pToken) => string.Empty;

    #region IPower Methods

    public void StartCharge(PlayerPowerManager powerManager, PowerToken pToken, bool startedChargingThisFrame)
    {
    }

    public void Charge(PlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    public void Release(PlayerPowerManager powerManager, PowerToken pToken, bool isCharged)
    {
    }

    public void Use(PlayerPowerManager powerManager, PowerToken pToken)
    {
        // Get the player enemy select
        var playerEnemySelect = powerManager.Player.PlayerEnemySelect;

        // If there is no enemy selected, return
        if (playerEnemySelect.SelectedEnemy == null)
            return;

        // Get the enemy info
        var enemy = playerEnemySelect.SelectedEnemy;

        // Get the enemy controller

        // Get the position of the shooter
        var shooterPosition = powerManager.Player.Rigidbody.position;

        // Get the position of the actor
        var actorPosition = enemy.GameObject.transform.position;

        // // Swap the positions of the shooter and the actor
        // // Get the enemy controller
        // var enemyMovement = enemyInfo.MovementBehavior;
        //
        // // Swap the positions of the shooter and the actor
        // enemyMovement.SetPosition(shooterPosition);

        enemy.NewMovement.SetPosition(shooterPosition);
        
        // Set the position of the actor to the shooter's position
        powerManager.Player.Rigidbody.MovePosition(actorPosition);

        // ====================================

        // // Instantiate the projectile prefab
        // var projectile = Instantiate(projectilePrefab, firePosition, Quaternion.identity);
        //
        // // Get the IPowerProjectile component from the projectile
        // var powerProjectile = projectile.GetComponent<IPowerProjectile>();
        //
        // // Shoot the projectile
        // powerProjectile.Shoot(this, powerManager, pToken, firePosition, fireForward);
    }

    #region Active Effects

    public void StartActiveEffect(PlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    public void UpdateActiveEffect(PlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    public void EndActiveEffect(PlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    #endregion

    #region Passive Effects

    public void StartPassiveEffect(PlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    public void UpdatePassiveEffect(PlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    public void EndPassiveEffect(PlayerPowerManager powerManager, PowerToken pToken)
    {
    }

    #endregion

    #endregion
}