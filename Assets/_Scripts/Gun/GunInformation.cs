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

    [SerializeField, Min(0)] private int cost;

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

    [Header("Sound")] [SerializeField] private Sound pickupSound;

    [SerializeField] private SoundPool fireSounds;

    [SerializeField] private Sound reloadSound;

    [Header("Recoil Settings")] [SerializeField] [Min(0)]
    private float horizontalRecoil;

    [Tooltip("Which direction the recoil will bias towards.")] [SerializeField, Range(-1, 1)]
    private float horizontalRecoilBias;

    [SerializeField] [Min(0)] private float verticalRecoil;

    [SerializeField, Range(0, 1)] private float minHorizontalRecoilPercent = 0.75f;
    [SerializeField, Range(0, 1)] private float minVerticalRecoilPercent = 0.75f;

    [SerializeField, Range(0, 1)] private float recoilLerpAmount = 0.1f;
    [SerializeField, Range(0, 1)] private float recoveryLerpAmount = 0.1f;

    [SerializeField] private DynamicNoiseModule.NoiseTokenValue recoilNoise;

    [SerializeField, Min(0)] private float recoilNoiseTime = 0.25f;
    [SerializeField, Range(0, 1)] private float recoilNoiseLerpAmount = 0.2f;

    #region Getters

    public string GunName => gunName;

    public int Cost => cost;

    public GunFireType FireType => gunFireType;

    public float BaseDamage => baseDamage;

    public float FireRate => fireRate;

    public float BloomAngle => bloomAngle;

    public float Range => range;

    public int MagazineSize => magazineSize;

    public float ReloadTime => reloadTime;

    #region Sounds

    public Sound PickupSound => pickupSound;

    public SoundPool FireSounds => fireSounds;

    public Sound ReloadSound => reloadSound;

    #endregion

    #region Recoil

    public float HorizontalRecoil => horizontalRecoil;

    public float VerticalRecoil => verticalRecoil;

    public float HorizontalRecoilBias => horizontalRecoilBias;

    public float RecoilLerpAmount => recoilLerpAmount;

    public float RecoveryLerpAmount => recoveryLerpAmount;

    public float MinHorizontalRecoilPercent => minHorizontalRecoilPercent;

    public float MinVerticalRecoilPercent => minVerticalRecoilPercent;

    public DynamicNoiseModule.NoiseTokenValue RecoilNoise => recoilNoise;

    public float RecoilNoiseTime => recoilNoiseTime;

    public float RecoilNoiseLerpAmount => recoilNoiseLerpAmount;

    #endregion

    #endregion

    public float EvaluateBaseDamage(float distance)
    {
        var distanceClamped = Mathf.Clamp(distance, 0, range);
        var distanceCoefficient = distanceClamped / range;

        return baseDamage * damageFalloffCurve.Evaluate(distanceCoefficient);
    }
}