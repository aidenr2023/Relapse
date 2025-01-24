using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class ChainLightning : MonoBehaviour, IPower
{
    // a field for a projectile prefab
    [SerializeField] private GameObject trailPrefab;

    [SerializeField, Min(0)] private float damage = 30f;
    [SerializeField, Min(0)] private int maxChainCount = 3;
    [SerializeField, Min(0)] private float maxChainDistance = 10f;
    [SerializeField, Min(0)] private float chainDelayTime = .25f;
    [SerializeField, Min(0)] private float chainStopTime = .25f;
    [SerializeField, Min(0)] private int chainStepCount = 3;

    [SerializeField, Min(0)] private float enemyStunTime = .5f;

    [SerializeField] private VisualEffect lightningVfxPrefab;

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
        // Start the chain lightning coroutine
        StartCoroutine(ChainLightningCoroutine(powerManager, pToken));
    }

    private IEnumerator ChainLightningCoroutine(
        PlayerPowerManager powerManager,
        PowerToken pToken
    )
    {
        var currentEnemy = powerManager.Player.PlayerEnemySelect.SelectedEnemy;

        if (currentEnemy == null)
            yield break;

        // Instantiate the trail prefab
        var trail = Instantiate(trailPrefab, powerManager.PowerFirePoint.position, Quaternion.identity);

        // Create a hash set of all the enemies
        var remainingEnemies = new HashSet<Enemy>();

        foreach (var enemy in Enemy.Enemies)
            remainingEnemies.Add(enemy);

        // Wait for 1 frame
        yield return null;

        var remainingChainCount = maxChainCount;

        var previousPosition = powerManager.PowerFirePoint.position;

        // Keep shooting the projectile at the current enemy
        while (remainingChainCount > 0)
        {
            // Remove the current enemy from the remaining enemies
            remainingEnemies.Remove(currentEnemy);

            // If the current enemy is null, break
            if (currentEnemy == null)
                break;

            // Get the position of the current enemy
            var enemyPosition = currentEnemy.transform.position;

            // TODO: Do more stuff here. Spawn a VFX, play a sound, etc.

            var remainingStepCount = chainStepCount;
            while (remainingStepCount > 0)
            {
                remainingStepCount--;

                // Lerp the position of the trail to the enemy position
                var trailPosition = Vector3.Lerp(previousPosition, enemyPosition,
                    1f - (remainingStepCount / (float)chainStepCount));

                // Set the position of the trail
                trail.transform.position = trailPosition;

                // Set the forward direction of the trail
                trail.transform.forward = enemyPosition - previousPosition;

                // Wait for the step delay time
                yield return new WaitForSeconds(chainDelayTime / chainStepCount);
            }

            if (currentEnemy != null)
            {
                // Damage the current enemy
                currentEnemy.EnemyInfo.ChangeHealth(-damage, powerManager.Player.PlayerInfo, this, enemyPosition);

                // Start the coroutine to stun the enemy
                StartCoroutine(StunEnemy(currentEnemy, enemyStunTime));
            }

            // Decrement the remaining chain count
            remainingChainCount--;

            var chainStopTimeStart = Time.time;

            // Wait for the chain stop time
            while (Time.time - chainStopTimeStart < chainStopTime)
            {
                // If the current enemy is null, break
                if (currentEnemy == null)
                {
                    yield return null;
                    continue;
                }

                // Update the position of the trail
                trail.transform.position = currentEnemy.transform.position;

                // Wait for 1 frame
                yield return null;
            }

            // Find the closest enemy to the current enemy
            if (remainingChainCount > 0)
                currentEnemy = GetClosestEnemy(enemyPosition, remainingEnemies);

            // Set the previous position to the current enemy position
            previousPosition = enemyPosition;
        }

        // Destroy the trail
        Destroy(trail.gameObject);
    }

    private IEnumerator StunEnemy(Enemy enemy, float stunTime)
    {
        // Add a movement disabled token to the enemy for the duration of the chain stop time
        enemy.EnemyMovementBehavior.AddMovementDisableToken(this);
        enemy.EnemyAttackBehavior.AddAttackDisableToken(this);

        // Instantiate the lightning VFX prefab
        var lightningVfx = Instantiate(lightningVfxPrefab);

        var stunStartTime = Time.time;
        
        while (Time.time - stunStartTime < stunTime)
        {
            if (enemy == null)
                break;
            
            // Set the position of the lightning VFX to the enemy position
            lightningVfx.transform.position = enemy.transform.position;

            // Wait for 1 frame
            yield return null;
        }
        
        // Remove the movement disabled token from the enemy
        if (enemy != null)
        {
            enemy.EnemyMovementBehavior.RemoveMovementDisableToken(this);
            enemy.EnemyAttackBehavior.RemoveAttackDisableToken(this);
            
        }
        
        // Destroy the lightning VFX
        if (lightningVfx != null)
        {
            lightningVfx.SetUInt("ChargeState", 0);
            Destroy(lightningVfx.gameObject, 10);
        }
    }

    private Enemy GetClosestEnemy(Vector3 currentPosition, HashSet<Enemy> remainingEnemies)
    {
        // Get the closest enemy to the current enemy
        Enemy closestEnemy = null;
        var closestDistance = float.MaxValue;

        foreach (var enemy in remainingEnemies)
        {
            var distance = Vector3.Distance(currentPosition,
                enemy.EnemyInfo.ParentComponent.transform.position);

            // Continue if the distance is greater than the max chain distance
            if (distance > maxChainDistance)
                continue;

            if (distance < closestDistance)
            {
                closestEnemy = enemy;
                closestDistance = distance;
            }
        }

        return closestEnemy;
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