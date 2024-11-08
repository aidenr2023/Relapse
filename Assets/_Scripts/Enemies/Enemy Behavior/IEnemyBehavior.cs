using UnityEngine;

public interface IEnemyBehavior
{
    public Enemy Enemy { get; }

    public GameObject GameObject { get; }
}