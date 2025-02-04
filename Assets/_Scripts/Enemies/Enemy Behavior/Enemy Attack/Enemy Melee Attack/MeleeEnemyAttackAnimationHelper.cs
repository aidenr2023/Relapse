using UnityEngine;

public class MeleeEnemyAttackAnimationHelper : EnemyAttackAnimationHelper
{
    private const float MOVEMENT_SPEED_MULTIPLIER = 1f;

    [SerializeField] private MeleeEnemyAttack meleeEnemyAttack;

    private TokenManager<float>.ManagedToken _movementSpeedToken;

    protected override object MovementDisableToken => meleeEnemyAttack;

    public override void ActivateHitBox(int index)
    {
        meleeEnemyAttack.ActivateHitBox(index);

        // Play the attack sound of the specific hit box
        meleeEnemyAttack.MeleeAttackHitboxes[index].PlayAttackSound();
    }

    public override void DeactivateHitBox(int index) => meleeEnemyAttack.DeactivateHitBox(index);

    public override void DeactivateMovement()
    {
        // meleeEnemyAttack.Enemy.EnemyMovementBehavior.AddMovementDisableToken(MovementDisableToken);

        // If a movement token already exists, remove it
        if (_movementSpeedToken != null)
            meleeEnemyAttack.Enemy.MovementBehavior.MovementSpeedTokens.RemoveToken(_movementSpeedToken);

        _movementSpeedToken = meleeEnemyAttack.Enemy.MovementBehavior
            .MovementSpeedTokens.AddToken(MOVEMENT_SPEED_MULTIPLIER, -1, true);
    }

    public override void ReactivateMovement()
    {
        // meleeEnemyAttack.Enemy.EnemyMovementBehavior.RemoveMovementDisableToken(MovementDisableToken);
        meleeEnemyAttack.Enemy.MovementBehavior.MovementSpeedTokens.RemoveToken(_movementSpeedToken);
    }
}