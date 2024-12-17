using UnityEngine;

public class MeleeEnemyAttackAnimationHelper : MonoBehaviour
{
    [SerializeField] private MeleeEnemyAttack meleeEnemyAttack;

    private object MovementDisableToken => meleeEnemyAttack;

    public void ActivateHitBox(int index) => meleeEnemyAttack.ActivateHitBox(index);
    public void DeactivateHitBox(int index) => meleeEnemyAttack.DeactivateHitBox(index);

    public void DeactivateMovement()
    {
        meleeEnemyAttack.Enemy.EnemyMovementBehavior.AddMovementDisableToken(MovementDisableToken);
    }

    public void ReactivateMovement()
    {
        meleeEnemyAttack.Enemy.EnemyMovementBehavior.RemoveMovementDisableToken(MovementDisableToken);
    }
}