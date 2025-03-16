using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Behavior Sub State", menuName = "Enemies/Behavior/01. Enemy Behavior Sub State")]
public class EnemyBehaviorSubStateHolder : EnemyBehaviorStateBase
{
    [SerializeField] public EnemyBehaviorStateBase[] subStates;
}