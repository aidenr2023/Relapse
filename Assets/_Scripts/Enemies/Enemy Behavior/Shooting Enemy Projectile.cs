using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingEnemyProjectile : MonoBehaviour
{
    [SerializeField] private float damage = 100f;

    private ShootingEnemyAttack _shootingEnemyAttack;
    private IDamager _damager;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if(!other.TryGetComponent(out IActor actor))
        {
            return;
        }
        actor.ChangeHealth(-damage, actor, _damager);
    }
    public void Shoot(IDamager damager, ShootingEnemyAttack shootingEnemyAttack)
    {
        _damager = damager;
        _shootingEnemyAttack = shootingEnemyAttack;
    }
}
