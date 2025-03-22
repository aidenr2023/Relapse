using UnityEngine;

public class MeleeEnemyAttackAnimationHelper : EnemyAttackAnimationHelper
{
    private const float MOVEMENT_SPEED_MULTIPLIER = 1f;

    [SerializeField] private MeleeEnemyAttack meleeEnemyAttack;

    private TokenManager<float>.ManagedToken _movementSpeedToken;

    protected override object MovementDisableToken => meleeEnemyAttack;

    public override void ActivateHitBox(int index)
    {
        // Return if the melee enemy attack is not enabled
        if (!meleeEnemyAttack.IsAttackEnabled)
            return;

        meleeEnemyAttack.ActivateHitBox(index);

        // Play the attack sound of the specific hit box
        meleeEnemyAttack.MeleeAttackHitboxes[index].PlayAttackSound();
    }

    public override void DeactivateHitBox(int index)
    {
        // Return if the shooting enemy attack is not enabled
        if (!meleeEnemyAttack.IsAttackEnabled)
            return;

        meleeEnemyAttack.DeactivateHitBox(index);
    }

    public override void DeactivateMovement()
    {
        // meleeEnemyAttack.Enemy.EnemyMovementBehavior.AddMovementDisableToken(MovementDisableToken);

        // If a movement token already exists, remove it
        if (_movementSpeedToken != null)
            meleeEnemyAttack.Enemy.NewMovement.MovementSpeedTokens.RemoveToken(_movementSpeedToken);

        _movementSpeedToken = meleeEnemyAttack.Enemy.NewMovement
            .MovementSpeedTokens.AddToken(MOVEMENT_SPEED_MULTIPLIER, -1, true);
    }

    public override void ReactivateMovement()
    {
        // meleeEnemyAttack.Enemy.EnemyMovementBehavior.RemoveMovementDisableToken(MovementDisableToken);
        meleeEnemyAttack.Enemy.NewMovement.MovementSpeedTokens.RemoveToken(_movementSpeedToken);
    }
}