using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Behavior Sub State", menuName = "Enemy Behavior/01. Enemy Behavior Sub State")]
public class EnemyBehaviorSubStateHolder : EnemyBehaviorStateBase
{
    [SerializeField] public EnemyBehaviorStateBase[] subStates;
}