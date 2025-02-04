using UnityEngine;

public class ShootingEnemyAttackAnimationHelper : EnemyAttackAnimationHelper
{
    [SerializeField] private ShootingEnemyAttack shootingEnemyAttack;

    protected override object MovementDisableToken => shootingEnemyAttack;

    public override void ActivateHitBox(int index)
    {
        // Fire the projectile
        shootingEnemyAttack.FireProjectile(index);
    }

    public override void DeactivateHitBox(int index)
    {
    }

    public override void DeactivateMovement()
    {
        shootingEnemyAttack.Enemy.MovementBehavior.AddMovementDisableToken(MovementDisableToken);
    }

    public override void ReactivateMovement()
    {
        shootingEnemyAttack.Enemy.MovementBehavior.RemoveMovementDisableToken(MovementDisableToken);
    }
}