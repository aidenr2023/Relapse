using System;
using UnityEngine;
using UnityEngine.VFX;

public class ExplosionHelper : MonoBehaviour, IDamager
{
    private const int MAX_COLLIDERS = 256;

    #region Serialized Fields

    [SerializeField, Min(0)] private float explosionRadius;
    [SerializeField, Min(0)] private float explosionDamage;

    [SerializeField] private VisualEffect explosionVfxPrefab;

    #endregion

    #region Getters

    public GameObject GameObject => gameObject;

    #endregion

    public void Explode()
    {
        // Create an array of colliders to store the colliders in the explosion radius
        var colliders = new Collider[MAX_COLLIDERS];

        // Sphere cast to get all the colliders in the explosion radius
        Physics.OverlapSphereNonAlloc(transform.position, explosionRadius, colliders);

        // Loop through all the colliders
        foreach (var cCollider in colliders)
        {
            // If the collider is null, continue
            if (cCollider == null)
                continue;

            // Try to get the IActor component from the collider
            if (!cCollider.TryGetComponentInParent(out IActor actor))
                continue;

            // Ray cast to see if the collider is in the line of sight
            var hit = Physics.Raycast(
                transform.position,
                cCollider.transform.position - transform.position,
                out var hitInfo,
                explosionRadius
            );

            // If the hit collider is not the collider we are checking, continue
            if (hit && hitInfo.collider != cCollider)
                continue;

            // Now, I can calculate damage
            actor.ChangeHealth(-explosionDamage, null, this, hitInfo.point);
        }

        // Instantiate the explosion VFX
        if (explosionVfxPrefab != null)
        {
            var explosionVfx = Instantiate(explosionVfxPrefab, transform.position, Quaternion.identity);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}