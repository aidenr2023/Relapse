using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Power", menuName = "Powers/Power")]
public class PowerScriptableObject : ScriptableObject
{
    [Header("Overview Information")] [SerializeField]
    private string name;

    [SerializeField] private PowerType powerType;
    [SerializeField] private Sprite icon;
    [SerializeField] [TextArea(3, 10)] private string description;

    [Header("Stats")] [SerializeField] private float cooldown;
    [SerializeField] private float activeDuration;
    [SerializeField] private float baseToleranceMeterImpact;
    [SerializeField] private float[] levelToleranceMultiplier;

    [SerializeField] private GameObject powerLogicPrefab;

    private IPower _powerLogic;

    #region Getters

    public string Name => name;
    public PowerType PowerType => powerType;
    public Sprite Icon => icon;
    public string Description => description;
    public float Cooldown => cooldown;
    public float ActiveDuration => activeDuration;
    public float BaseToleranceMeterImpact => baseToleranceMeterImpact;
    public float[] LevelToleranceMultiplier => levelToleranceMultiplier;
    public GameObject PowerLogicPrefab => powerLogicPrefab;
    public IPower PowerLogic => _powerLogic;

    #endregion

    private void Awake()
    {
        // Initialize the components
        InitializeComponents();

        // Validate the power
        ValidatePower();
    }

    private void InitializeComponents()
    {
        // Get the PowerLogic component
        _powerLogic = powerLogicPrefab.GetComponent<IPower>();
    }

    /// <summary>
    /// A method to make sure the power was set up correctly.
    /// </summary>
    private void ValidatePower()
    {
        // Make sure the PowerLogic is not null
        Debug.Assert(_powerLogic != null,
            "PowerLogic is null. Please assign a PowerLogic prefab to the PowerScriptableObject."
        );
    }
}