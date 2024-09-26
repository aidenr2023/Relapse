using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "GunInformation", menuName = "Gun/Gun Information", order = 0)]
public class GunInformation : ScriptableObject
{
    public enum GunFireType
    {
        SemiAutomatic,
        Automatic
    }

    [SerializeField] private string gunName;

    [SerializeField] private GunFireType gunFireType;

    [SerializeField] private float baseDamage;

    [Tooltip("How many shots per second the gun can fire.")] [SerializeField]
    private float fireRate;

    [Tooltip("The angle of the bloom when firing the gun.")] [Range(0, 90)] [SerializeField]
    private float bloomAngle;

    [SerializeField] [Min(0)] private float range;
    
    [SerializeField] private AnimationCurve damageFalloffCurve;

    #region Getters

    public string GunName => gunName;

    public GunFireType FireType => gunFireType;

    public float BaseDamage => baseDamage;

    public float FireRate => fireRate;

    public float BloomAngle => bloomAngle;

    public float Range => range;
    
    #endregion
    
    public float EvaluateBaseDamage(float distance)
    {
        var distanceClamped = Mathf.Clamp(distance, 0, range);
        var distanceCoefficient = distanceClamped / range;
        
        return baseDamage * damageFalloffCurve.Evaluate(distanceCoefficient);
    }
}