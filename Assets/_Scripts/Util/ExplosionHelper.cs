﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class ExplosionHelper : MonoBehaviour, IDamager
{
    private const int MAX_COLLIDERS = 256;

    #region Serialized Fields

    [SerializeField, Min(0)] private float explosionRadius;
    [SerializeField, Min(0)] private float explosionDamage;
    [SerializeField, Min(0)] private float damageMultiplier = 1;

    [SerializeField] private CameraShakeHelper cameraShakeHelper;

    [SerializeField] private ParticleSystem explosionParticlePrefab;
    [SerializeField] private VisualEffect explosionVfxPrefab;
    [SerializeField] private Sound sound;

    #endregion

    #region Getters

    public GameObject GameObject => gameObject;

    public Sound NormalHitSfx => null;
    public Sound CriticalHitSfx => null;
    
    public float Damage => explosionDamage;

    #endregion

    public void Explode(bool canDamagePlayer)
    {
        Explode(explosionDamage, null, this, canDamagePlayer);
    }

    public void Explode() => Explode(false);

    public void Explode(float amount, IActor changer, IDamager damager, bool canDamagePlayer)
    {
        // Create an array of colliders to store the colliders in the explosion radius
        var colliders = new Collider[MAX_COLLIDERS];

        // Sphere cast to get all the colliders in the explosion radius
        Physics.OverlapSphereNonAlloc(transform.position, explosionRadius, colliders);

        var actors = new HashSet<IActor>();

        // Loop through all the colliders
        foreach (var cCollider in colliders)
        {
            // If the collider is null, continue
            if (cCollider == null)
                continue;

            // Try to get the IActor component from the collider
            if (!cCollider.TryGetComponentInParent(out IActor actor))
                continue;

            // Continue if the actor is a player
            if (actor is PlayerInfo && !canDamagePlayer)
                continue;

            if (!actors.Add(actor))
                continue;

            // Now, I can calculate damage
            actor.ChangeHealth(-amount * damageMultiplier, changer, damager, actor.GameObject.transform.position);
        }

        // Instantiate the explosion VFX
        if (explosionVfxPrefab != null)
        {
            var explosionVfx = Instantiate(explosionVfxPrefab, transform.position, Quaternion.identity);

            explosionVfx.Play();

            Destroy(explosionVfx, 10);
        }

        // Instantiate the explosion particle system
        if (explosionParticlePrefab != null)
        {
            var explosionParticle = Instantiate(explosionParticlePrefab, transform.position, Quaternion.identity);
            explosionParticle.Play();

            // Set it to destroy itself after the duration of the particle system
            Destroy(explosionParticle.gameObject, explosionParticle.main.duration);
        }

        // Play the sound
        if (sound != null)
            SoundManager.Instance.PlaySfxAtPoint(sound, transform.position);

        // Shake the camera (if the camera shake helper is not null)
        if (cameraShakeHelper != null)
            cameraShakeHelper.ShakeCamera();
    }

    public void SetDamageMultiplier(float multiplier) => damageMultiplier = multiplier;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}