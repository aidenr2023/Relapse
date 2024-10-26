using System;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Power", menuName = "Power")]
public class PowerScriptableObject : ScriptableObject
{
    [Header("Overview Information")] [SerializeField]
    private string powerName;

    [SerializeField] private PowerType powerType;
    [SerializeField] private Sprite icon;
    [SerializeField] [TextArea(3, 10)] private string description;

    [Header("Stats")] [SerializeField] private float chargeDuration;
    [SerializeField] private float activeEffectDuration;
    [SerializeField] private float passiveEffectDuration;
    [SerializeField] private float cooldown;
    [SerializeField] private float baseToleranceMeterImpact;
    [SerializeField] private float[] toleranceMeterLevelMultiplier = { 1, .75f, .5f };

    [SerializeField] private GameObject powerLogicPrefab;

    private IPower _powerLogic;

    #region Sounds

    [Header("Sounds")]
    [SerializeField] private Sound chargeStartSound;

    [SerializeField] private Sound powerUseSound;

    [SerializeField] private Sound powerReadySound;

    #endregion

    #region Getters

    public string PowerName => powerName;
    public PowerType PowerType => powerType;
    public Sprite Icon => icon;
    public string Description => description;
    public float ChargeDuration => chargeDuration;
    public float Cooldown => cooldown;
    public float ActiveEffectDuration => activeEffectDuration;
    public float PassiveEffectDuration => passiveEffectDuration;
    public float BaseToleranceMeterImpact => baseToleranceMeterImpact;
    public float[] ToleranceMeterLevelMultiplier => toleranceMeterLevelMultiplier;
    public GameObject PowerLogicPrefab => powerLogicPrefab;

    public IPower PowerLogic
    {
        get
        {
            EnsureLoad();
            return _powerLogic;
        }
    }

    public int MaxLevel => toleranceMeterLevelMultiplier.Length - 1;

    #region Sound

    public Sound ChargeStartSound => chargeStartSound;

    public Sound PowerUseSound => powerUseSound;

    public Sound PowerReadySound => powerReadySound;

    #endregion

    #endregion

    private void OnValidate()
    {
        // Validate the power
        ValidatePower();
    }

    private void InitializeComponents()
    {
        // Get the PowerLogic component
        _powerLogic = Instantiate(powerLogicPrefab).GetComponent<IPower>();

        // Attach the PowerScriptableObject to the PowerLogic
        _powerLogic.AttachToScriptableObject(this);

        // Hide the PowerLogic object in the hierarchy
        _powerLogic.GameObject.hideFlags = HideFlags.HideInHierarchy;
    }

    /// <summary>
    /// A method to make sure the power was set up correctly.
    /// </summary>
    private void ValidatePower()
    {
        // // Make sure the PowerLogic is not null
        // Debug.Assert(powerLogicPrefab != null,
        //     "PowerLogic is null. Please assign a PowerLogic prefab to the PowerScriptableObject."
        // );
    }

    /// <summary>
    /// Make sure the prefab associated with the power has been loaded.
    /// </summary>
    private void EnsureLoad()
    {
        // Skip if the power is loaded
        if (_powerLogic != null)
            return;

        // Initialize the components
        InitializeComponents();
    }
}