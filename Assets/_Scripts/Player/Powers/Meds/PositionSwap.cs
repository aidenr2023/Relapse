using System.Collections;
using UnityEngine;

public class PositionSwap : MonoBehaviour, IPower
{
    // a field for a projectile prefab
    [SerializeField] private GameObject projectilePrefab;

    [SerializeField] private LayerMask layersToHit;

    [SerializeField] private AnimationCurve startTimeScaleCurve;
    [SerializeField] private AnimationCurve endTimeScaleCurve;

    [SerializeField] private ParticleSystem oldPositionParticleSystemPrefab;
    [SerializeField] [Range(0, 500)] private int particlesCount = 200;

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
        if (!playerEnemySelect.SelectedEnemy.HasValue)
            return;

        // Get the enemy info
        var enemy = playerEnemySelect.SelectedEnemy.Value;

        // Get the enemy controller

        // Get the position of the shooter
        var shooterPosition = powerManager.Player.Rigidbody.position;

        // // Swap the positions of the shooter and the actor
        // // Get the enemy controller
        // var enemyMovement = enemyInfo.MovementBehavior;
        //
        // // Swap the positions of the shooter and the actor
        // enemyMovement.SetPosition(shooterPosition);

        // Start the position swap coroutine
        StartCoroutine(PositionSwapCoroutine(powerManager, pToken, enemy, shooterPosition));

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

    private IEnumerator PositionSwapCoroutine(
        PlayerPowerManager powerManager, PowerToken pToken,
        Enemy enemy, Vector3 shooterPosition
    )
    {
        var initialWaitTime = startTimeScaleCurve.keys[^1].time;
        var endWaitTime = endTimeScaleCurve.keys[^1].time;

        var timeToken = TimeScaleManager.Instance.TimeScaleTokenManager.AddToken(.125f, -1, true);

        // Evaluate the time scale curve
        var startTime = Time.unscaledTime;
        while (Time.unscaledTime - startTime < initialWaitTime)
        {
            timeToken.Value = startTimeScaleCurve.Evaluate(Time.unscaledTime - startTime);
            yield return null;
        }

        timeToken.Value = startTimeScaleCurve.keys[^1].value;

        if (enemy != null && enemy.gameObject != null)
        {
            // Get the position of the actor
            var actorPosition = enemy.GameObject.transform.position;

            // Get the forward direction of the enemy
            var enemyForward = shooterPosition - actorPosition;
            enemyForward = new Vector3(enemyForward.x, 0, enemyForward.z).normalized;

            // Set the position of the enemy to the shooter's position
            enemy.NewMovement.SetPosition(shooterPosition);

            // Instantiate the old position particle system prefab
            CreateParticles(actorPosition);
            CreateParticles(shooterPosition);

            // Set the position of the actor to the shooter's position
            powerManager.Player.Rigidbody.MovePosition(actorPosition);

            // Set the forward direction of the player
            powerManager.Player.PlayerLook.ApplyRotation(Quaternion.LookRotation(enemyForward));
        }

        // Evaluate the time scale curve
        startTime = Time.unscaledTime;
        while (Time.unscaledTime - startTime < endWaitTime)
        {
            timeToken.Value = endTimeScaleCurve.Evaluate(Time.unscaledTime - startTime);
            yield return null;
        }

        timeToken.Value = endTimeScaleCurve.keys[^1].value;

        // Remove the time token
        TimeScaleManager.Instance.TimeScaleTokenManager.RemoveToken(timeToken);

        yield return null;
    }

    private void CreateParticles(Vector3 position)
    {
        // Instantiate the explosion particles at the projectile's position
        var explosion = Instantiate(oldPositionParticleSystemPrefab, position, Quaternion.identity);

        // Create emit parameters for the explosion particles
        var emitParams = new ParticleSystem.EmitParams
        {
            applyShapeToPosition = true,
            position = position
        };

        // Emit the explosion particles
        explosion.Emit(emitParams, particlesCount);

        // Destroy the explosion particles after the duration of the particles
        Destroy(explosion.gameObject, explosion.main.duration);
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