using UnityEngine;

public class HordeModeSpawner : EnemySpawner
{
    #region Serialized Fields
    
    
    
    #endregion
    
    #region EnemySpawner Implementation

    protected override string GetTooltipText()
    {
        return "TOOLTIP TEXT";
    }

    protected override void CustomStart()
    {
        throw new System.NotImplementedException();
    }

    protected override void CustomDestroy()
    {
        throw new System.NotImplementedException();
    }

    protected override void CustomSpawnEnemy(Enemy actualEnemy, Vector3 spawnPosition, Quaternion spawnRotation)
    {
        throw new System.NotImplementedException();
    }

    protected override void CustomStartSpawning()
    {
        throw new System.NotImplementedException();
    }

    protected override void CustomStopSpawning()
    {
        throw new System.NotImplementedException();
    }

    #endregion
}