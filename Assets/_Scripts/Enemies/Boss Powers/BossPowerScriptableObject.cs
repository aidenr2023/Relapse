using UnityEngine;

[CreateAssetMenu(fileName = "BossPower", menuName = "Boss Power")]
public class BossPowerScriptableObject : ScriptableObject
{
    #region Serialized Fields
    
    [SerializeField] private PowerScriptableObject correspondingPower;
    
    /// <summary>
    /// How does the enemy move AFTER they decide to use this power, but BEFORE they use it?
    /// </summary>
    [SerializeField] private BossPowerMovementBehavior beforeUseMovementBehavior;
    
    #endregion
    
    #region Getters
    
    
    
    #endregion

    public enum BossPowerMovementBehavior
    {
        None,
        GetCloser,
        GetAway,
        GoToSpecificLocation,
    }
}
