using UnityEngine;

public class MeleeEnemyAttackAnimationHelper : EnemyAttackAnimationHelper
{
    [SerializeField] private MeleeEnemyAttack meleeEnemyAttack;

    protected override object MovementDisableToken => meleeEnemyAttack;

    public override void ActivateHitBox(int index) => meleeEnemyAttack.ActivateHitBox(index);
    public override void DeactivateHitBox(int index) => meleeEnemyAttack.DeactivateHitBox(index);

    public override void DeactivateMovement()
    {
        meleeEnemyAttack.Enemy.EnemyMovementBehavior.AddMovementDisableToken(MovementDisableToken);
    }

    public override void ReactivateMovement()
    {
        meleeEnemyAttack.Enemy.EnemyMovementBehavior.RemoveMovementDisableToken(MovementDisableToken);
    }
}