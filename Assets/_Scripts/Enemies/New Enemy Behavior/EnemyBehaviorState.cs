using System;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Enemy Behavior State", menuName = "Enemy Behavior/00. Enemy Behavior State")]
public class EnemyBehaviorState : EnemyBehaviorStateBase
{
    [SerializeField] public BehaviorActionAttack[] attackActions;
    [SerializeField] public BehaviorActionMove[] moveActions;
}