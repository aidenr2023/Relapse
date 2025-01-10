using System.Collections.Generic;
using UnityEngine;

public class GravityMineProjectile : AbstractMineProjectile
{
    #region Serialized Fields

    [SerializeField] private float pullForce = 10f;
    [SerializeField, Min(0)] private float pullDuration = 3;

    #endregion

    #region Private Fields

    private CountdownTimer _pullTimer;

    #endregion

    protected override void CustomAwake()
    {
        // Create the pull timer
        _pullTimer = new CountdownTimer(pullDuration);
        _pullTimer.OnTimerEnd += () => Destroy(gameObject);
    }

    protected override void CustomUpdate()
    {
        // Update the pull timer
        _pullTimer.SetMaxTime(pullDuration);
        _pullTimer.Update(Time.deltaTime);
    }

    protected override void OnStartFuse()
    {
    }

    protected override void CustomFixedUpdate()
    {
        // Pull enemies towards the mine
        PullEnemies();
    }

    private void PullEnemies()
    {
        // Return if the pull timer is not active
        if (!_pullTimer.IsActive)
            return;

        var enemies = new Collider[MAX_ENEMIES];

        // Get the game objects in the enemyLayers
        var enemyCount = Physics.OverlapSphereNonAlloc(transform.position, detectionRadius, enemies, enemyLayers);

        var actorSet = new HashSet<IActor>();

        // For each enemy, get the actor component
        for (var i = 0; i < enemyCount; i++)
        {
            // Get the enemy
            var enemy = enemies[i];

            // Get the actor component from the enemy
            var actor = enemy.GetComponentInParent<IActor>();

            // Continue if the actor is null
            if (actor == null)
                continue;

            // If the actor is the shooter, continue
            if (actor == _shooter)
                continue;

            actorSet.Add(actor);
        }

        // Return if there are no enemies
        if (actorSet.Count == 0)
            return;

        // Pull the enemies towards the mine
        foreach (var actor in actorSet)
        {
            // Get the direction to the enemy
            var direction = transform.position - actor.GameObject.transform.position;

            // Get the rigidbody component from the actor
            if (!actor.GameObject.TryGetComponent(out Rigidbody rb))
                continue;

            var cPullForce = pullForce;

            // If the actor is close enough to the mine, reduce the pull force
            if (direction.magnitude < pullForce * Time.fixedDeltaTime)
                cPullForce = direction.magnitude / Time.fixedDeltaTime;

            rb.AddForce(direction.normalized * cPullForce, ForceMode.VelocityChange);
        }
    }

    protected override void ApplyExplosion(IActor actor)
    {
        // Start the pull timer
        _pullTimer.SetMaxTimeAndReset(pullDuration);
        _pullTimer.Start();
    }

    protected override void CustomShoot(IPower power, PlayerPowerManager powerManager, PowerToken pToken,
        Vector3 position, Vector3 forward)
    {
    }
}