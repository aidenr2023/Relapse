using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "GunInformation", menuName = "Gun Information", order = 0)]
public class GunInformation : ScriptableObject
{
    public enum GunFireType
    {
        SemiAutomatic,
        Automatic
    }

    [Header("Gun Information")] [SerializeField]
    private string gunName;

    [SerializeField] private GunFireType gunFireType;

    [Header("Stats")] [SerializeField] private float baseDamage;

    [Tooltip("How many shots per second the gun can fire.")] [SerializeField]
    private float fireRate;

    [Tooltip("The angle of the bloom when firing the gun.")] [Range(0, 180)] [SerializeField]
    private float bloomAngle;

    [SerializeField] [Min(0)] private float range;

    [Tooltip("How many bullets can be fired before needing to reload.")] [SerializeField] [Min(1)]
    private int magazineSize;

    [Tooltip("How long it takes (in seconds) to reload the gun.")] [SerializeField] [Min(0)]
    private float reloadTime;

    [SerializeField] private AnimationCurve damageFalloffCurve;

    #region Getters

    public string GunName => gunName;

    public GunFireType FireType => gunFireType;

    public float BaseDamage => baseDamage;

    public float FireRate => fireRate;

    public float BloomAngle => bloomAngle;

    public float Range => range;

    public int MagazineSize => magazineSize;

    public float ReloadTime => reloadTime;

    #endregion

    public float EvaluateBaseDamage(float distance)
    {
        var distanceClamped = Mathf.Clamp(distance, 0, range);
        var distanceCoefficient = distanceClamped / range;

        return baseDamage * damageFalloffCurve.Evaluate(distanceCoefficient);
    }
}