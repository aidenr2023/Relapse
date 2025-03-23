using System;
using System.Collections;
using UnityEngine;

public class BossVirusCloud : MonoBehaviour
{
    #region Private Fields

    private BossVirusBehavior _virusBehavior;

    #endregion

    public void Initialize(BossVirusBehavior virusBehavior)
    {
        _virusBehavior = virusBehavior;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Return if there isn't an actor component
        if (!other.TryGetComponentInParent(out IActor actor))
            return;

        // Return if the actor isn't a player
        if (actor is not PlayerInfo player)
            return;

        // Return if the player is already infected
        if (_virusBehavior.IsActorInfected(player))
            return;

        // Add the player to the boss virus's infected actors
        _virusBehavior.AddInfectedActor(player);

        // Start the virus coroutine
        _virusBehavior.StartCoroutine(VirusCoroutine(player));
    }

    private IEnumerator VirusCoroutine(IActor actor)
    {
        var startTime = Time.time;

        while (Time.time - startTime < _virusBehavior.TickDuration)
        {
            yield return new WaitForSeconds(_virusBehavior.TickDelay);
            
            // Damage the actor
            actor.ChangeHealth(
                -_virusBehavior.TickDamage,
                _virusBehavior.BossEnemyAttack.ParentComponent.ParentComponent,
                _virusBehavior.BossEnemyAttack,
                actor.GameObject.transform.position
            );
        }
        
        // Remove the actor from the infected actors list
        _virusBehavior.RemoveInfectedActor(actor);
    }
}