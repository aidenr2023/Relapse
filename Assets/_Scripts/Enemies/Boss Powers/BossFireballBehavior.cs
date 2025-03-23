using System.Collections;
using UnityEngine;

public class BossFireballBehavior : BossPowerBehavior
{
    // Fireball projectile prefab
    
    protected override void CustomInitialize(BossEnemyAttack bossEnemyAttack)
    {
    }

    protected override IEnumerator CustomUsePower()
    {
        yield return new WaitForSeconds(3);
    }
}