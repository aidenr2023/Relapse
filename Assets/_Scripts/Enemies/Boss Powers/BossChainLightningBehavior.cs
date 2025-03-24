using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BossChainLightningBehavior : BossPowerBehavior
{
    #region Serialized Fields

    [SerializeField] private BossChainLightningProjectile projectilePrefab;
    [SerializeField] private Transform[] lightningPoints;

    [SerializeField] private float lifeTime = 5;

    [field: SerializeField, Min(0)] public float Damage { get; private set; } = 20f;

    [field: SerializeField, Min(0)] public float MoveDelay { get; private set; } = 1 / 8f;
    [field: SerializeField, Min(0)] public float MaxMoveDistance { get; private set; } = 4f;
    [field: SerializeField, Min(0)] public float MaxMoveAngle { get; private set; } = 15;

    #endregion

    #region Private Fields

    private readonly Dictionary<Transform, BossChainLightningProjectile> _projectiles = new();

    #endregion

    protected override void CustomInitialize(BossEnemyAttack bossEnemyAttack)
    {
    }

    protected override IEnumerator CustomUsePower()
    {
        // Set the movement mode to idle
        BossEnemyAttack.ParentComponent.SetBossBehaviorMode(BossBehaviorMode.Idle);

        // Create the projectiles
        yield return StartCoroutine(CreateProjectiles());

        // Set the movement mode to chase maintain distance
        BossEnemyAttack.ParentComponent.SetBossBehaviorMode(BossBehaviorMode.ChaseMaintainDistance);

        // Play the power ready particles
        PlayPowerReadyParticles();

        // Wait for a second
        yield return new WaitForSeconds(1);

        // Shoot the projectiles
        yield return StartCoroutine(ShootProjectiles());

        yield return null;
    }

    private IEnumerator CreateProjectiles()
    {
        // Create an array in a random order of the lightning points
        var randomLightningPoints = new List<Transform>(lightningPoints);

        // Shuffle the array
        for (var i = 0; i < randomLightningPoints.Count; i++)
        {
            var temp = randomLightningPoints[i];
            var randomIndex = Random.Range(i, randomLightningPoints.Count);
            randomLightningPoints[i] = randomLightningPoints[randomIndex];
            randomLightningPoints[randomIndex] = temp;
        }

        var coroutines = new Dictionary<Transform, Coroutine>();

        var targetTransform = BossEnemyAttack.Enemy.DetectionBehavior.Target.GameObject.transform;

        // For each lightning point
        foreach (var point in randomLightningPoints)
        {
            // Instantiate the chain lightning projectile
            var projectile = Instantiate(projectilePrefab, point);
            _projectiles[point] = projectile;

            // Create the projectile
            coroutines[point] = StartCoroutine(projectile.CreateProjectile(this, 0, targetTransform));

            // Wait a little to create the next projectile
            yield return new WaitForSeconds(1 / 8f);
        }

        // Yield while there are any projectiles that are not done creating yet
        yield return new WaitWhile(() => coroutines.Keys.Any(point => !_projectiles[point].IsDoneBeingCreated));

        // Log that the routine is done
        Debug.Log("Done creating projectiles");
    }

    private IEnumerator ShootProjectiles()
    {
        // Create an array in a random order of the lightning points
        var randomLightningPoints = new List<Transform>(lightningPoints);

        // Shuffle the array
        for (var i = 0; i < randomLightningPoints.Count; i++)
        {
            var temp = randomLightningPoints[i];
            var randomIndex = Random.Range(i, randomLightningPoints.Count);
            randomLightningPoints[i] = randomLightningPoints[randomIndex];
            randomLightningPoints[randomIndex] = temp;
        }

        // For each projectile
        foreach (var point in randomLightningPoints)
        {
            // Get the corresponding projectile
            var projectile = _projectiles[point];

            // Set the projectile to destroy after a certain amount of time
            Destroy(projectile.gameObject, lifeTime);

            // Shoot the projectile
            yield return StartCoroutine(projectile.ShootProjectile());
        }

        yield return null;
    }
}