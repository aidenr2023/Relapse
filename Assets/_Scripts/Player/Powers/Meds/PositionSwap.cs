using UnityEngine;

public class PositionSwap : MonoBehaviour, IPower
{
    // a field for a projectile prefab
    [SerializeField] private GameObject projectilePrefab;

    [SerializeField] private LayerMask layersToHit;

    public GameObject GameObject => gameObject;
    public PowerScriptableObject PowerScriptableObject { get; set; }

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
        var enemyInfo = playerEnemySelect.SelectedEnemy;

        // Get the enemy controller

        // Get the position of the shooter
        var shooterPosition = powerManager.Player.Rigidbody.position;

        // Get the position of the actor
        var actorPosition = enemyInfo.GameObject.transform.position;

        // Swap the positions of the shooter and the actor
        // Get the enemy controller
        var enemyMovement = enemyInfo.EnemyMovementBehavior;

        // Swap the positions of the shooter and the actor
        enemyMovement.SetPosition(shooterPosition);

        // Set the position of the actor to the shooter's position
        powerManager.Player.Rigidbody.MovePosition(actorPosition);

        // ====================================

        // // Create the position of the projectile
        // var firePosition = powerManager.PowerFirePoint.position;
        //
        // // Create a vector that points forward from the camera pivot
        // var aimTargetPoint = powerManager.PowerAimHitPoint;
        // var fireForward = (aimTargetPoint - firePosition).normalized;
        //
        // // Fire a raycast from the fire position to the aim target point
        // var ray = new Ray(firePosition, fireForward);
        // var rayDistance = Vector3.Distance(firePosition, aimTargetPoint);
        //
        // var hit = Physics.Raycast(ray, out var hitInfo, rayDistance, layersToHit);
        //
        // if (!hit)
        //     return;
        //
        // // If the projectile hits something with an IActor component, deal damage
        // if (hitInfo.collider.TryGetComponentInParent(out IActor actor))
        // {
        //     // Get the position of the shooter
        //     var shooterPosition = firePosition;
        //
        //     // Get the position of the actor
        //     var actorPosition = actor.GameObject.transform.position;
        //
        //     // Swap the positions of the shooter and the actor
        //     if (actor is EnemyInfo enemyInfo)
        //     {
        //         // Get the enemy controller
        //         var enemyMovement = enemyInfo.ParentComponent.EnemyMovementBehavior;
        //
        //         // Swap the positions of the shooter and the actor
        //         enemyMovement.SetPosition(shooterPosition);
        //     }
        //
        //     // Set the position of the actor to the shooter's position
        //     powerManager.Player.Rigidbody.MovePosition(actorPosition);
        // }

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