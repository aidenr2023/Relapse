using UnityEngine;

public abstract class EnemyAttackAnimationHelper : MonoBehaviour
{
    protected abstract object MovementDisableToken { get; }

    public abstract void ActivateHitBox(int index);

    public abstract void DeactivateHitBox(int index);

    public abstract void DeactivateMovement();

    public abstract void ReactivateMovement();
}