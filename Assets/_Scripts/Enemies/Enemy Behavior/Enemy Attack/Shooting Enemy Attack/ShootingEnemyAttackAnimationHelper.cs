using UnityEngine;

public class ShootingEnemyAttackAnimationHelper : EnemyAttackAnimationHelper
{
    [SerializeField] private ShootingEnemyAttack shootingEnemyAttack;

    protected override object MovementDisableToken => shootingEnemyAttack;

    public override void ActivateHitBox(int index)
    {
        // Return if the shooting enemy attack is not enabled
        if (!shootingEnemyAttack.IsAttackEnabled)
            return;
        
        // Fire the projectile
        shootingEnemyAttack.FireProjectile(index);
    }

    public override void DeactivateHitBox(int index)
    {
        // Return if the shooting enemy attack is not enabled
        if (!shootingEnemyAttack.IsAttackEnabled)
            return;
        
    }

    public override void DeactivateMovement()
    {
        // shootingEnemyAttack.Enemy.MovementBehavior.AddMovementDisableToken(MovementDisableToken);
        shootingEnemyAttack.Enemy.NewMovement.AddMovementDisableToken(MovementDisableToken);
    }

    public override void ReactivateMovement()
    {
        // shootingEnemyAttack.Enemy.MovementBehavior.RemoveMovementDisableToken(MovementDisableToken);
        shootingEnemyAttack.Enemy.NewMovement.RemoveMovementDisableToken(MovementDisableToken);
    }
}