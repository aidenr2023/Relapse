using System;
using UnityEngine;

public class FallingObstacle : MonoBehaviour, IDamager
{
    [SerializeField] private float damage;

    public GameObject GameObject => gameObject;

    public Sound NormalHitSfx => null;
    public Sound CriticalHitSfx => null;
    
    private void OnCollisionEnter(Collision other)
    {
        // Get the actor component from the other object
        if (!other.transform.TryGetComponentInParent(out IActor actor))
            return;

        var mostDownwardNormal = other.contacts[0].normal;

        // Loop through all the contacts to find the most downward normal
        for (var i = 1; i < other.contacts.Length; i++)
        {
            if (other.contacts[i].normal.y < mostDownwardNormal.y)
                mostDownwardNormal = other.contacts[i].normal;
        }

        // If the normal of the collision is pointing too upwards, return
        if (mostDownwardNormal.y > 0)
            return;

        Debug.Log(
            $"Thing ({other.gameObject.name}): {other.relativeVelocity} - {other.relativeVelocity.magnitude:0.00} {mostDownwardNormal.y:0.00}");

        // Deal damage to the actor
        actor.ChangeHealth(-damage, null, this, other.contacts[0].point);
    }
}