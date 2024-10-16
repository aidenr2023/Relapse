using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour, IDamager
{
    [SerializeField] private bool bulletSpread = true;

    [SerializeField] private Vector3 bulletSpreadVariance = new Vector3(2f, 2f, 2f);


    public ParticleSystem impactParticleSystem;

    public Transform bulletSpawnLocation;
    public float shotDelay = 0.5f;
    private float lastShot;
    public LayerMask mask;
    public AudioSource hitSound;
    public PlayerInfo pi;

    public GameObject GameObject => gameObject;

    public void Shoot()
    {
        //firing weapon and delay between shots
        if (lastShot + shotDelay < Time.time)
        {
            Vector3 direction = GetBulletTrajectory();
            if (Physics.Raycast(bulletSpawnLocation.position, direction, out RaycastHit hit, float.MaxValue, mask))
            {
                OnHit(hit);
                hitSound.Play();
                lastShot = Time.time;
            }
        }
    }

    private Vector3 GetBulletTrajectory()
    {
        Vector3 direction = transform.forward;

        // means of calculating bullet spread based on a range of a objects normal
        if (bulletSpread)
        {
            direction += new Vector3(
                Random.Range(-bulletSpreadVariance.x, bulletSpreadVariance.x),
                Random.Range(-bulletSpreadVariance.y, bulletSpreadVariance.y),
                Random.Range(-bulletSpreadVariance.z, bulletSpreadVariance.z)
            );

            direction.Normalize();
        }

        return direction;
    }

    private void OnHit(RaycastHit hit)
    {
        Instantiate(impactParticleSystem, hit.point, Quaternion.LookRotation(hit.normal));
        if (hit.collider.CompareTag("Enemy"))
        {
            // Attempt to find an EnemyHealth component and deal damage
            EnemyInfo enemyInfoHealth = hit.collider.GetComponent<EnemyInfo>();
            if (enemyInfoHealth != null)
            {
                // Deal 2 points of damage, adjust as needed
                enemyInfoHealth.ChangeHealth(-2, pi, this);
            }

            // if (hitSound != null)
            // {
            //     hitSound.Play();
            // }
        }
    }
}