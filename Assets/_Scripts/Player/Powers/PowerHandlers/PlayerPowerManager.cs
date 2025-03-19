using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.VFX;

[RequireComponent(typeof(Player))]
public class PlayerPowerManager : MonoBehaviour, IDebugged, IUsesInput, IPlayerLoaderInfo
{
    private const float MAX_FADE_TIME = .25f;

    public static PlayerPowerManager Instance { get; private set; }

    #region Serialized Fields

    [SerializeField] private PowerTokenListVariable powerTokensSo;

    [SerializeField] private Transform powerFirePoint;

    [SerializeField] private LayerMask powerAimIgnoreLayers;

    [SerializeField] private PowerListReference startingPowers;
    [SerializeField] private PowerListReference allPowers;
    [SerializeField] private PowerListReference powers;
    [SerializeField] private IntReference currentPowerIndex;

    [Header("Power Charged Vignette"), SerializeField, Range(0, 1)]
    private float chargedVignetteStrength = .25f;

    [SerializeField, Range(0, 1)] private float chargedVignetteLerpAmount = .25f;
    [SerializeField, Min(0)] private float chargedVignetteFlashesPerSecond = 1f;

    [Header("Visual Effects")] [SerializeField]
    private VisualEffect fireballChargeVfx;

    [SerializeField] private VisualEffect greenFireballChargeVfx;

    [SerializeField] private VisualEffect electricChargeVfx;
    [SerializeField] private VisualEffect healthHaloChargeVfx;

    [Header("Sound Effects")] [SerializeField]
    private ManagedAudioSource powerAudioSource;

    #endregion

    public event Action<PlayerPowerManager, PowerToken> OnPowerUsed;

    #region Private Fields

    private Player _player;

    private Dictionary<PowerScriptableObject, PowerToken> _powerTokens;

    private bool _isChargingPower;

    private TokenManager<float>.ManagedToken _powerChargeVignetteToken;

    private bool _isPowerAimHitting;
    private RaycastHit _powerAimHit;

    private readonly Dictionary<VisualEffect, float> _fadeTimes = new();

    #endregion

    #region Getters

    public HashSet<InputData> InputActions { get; } = new();

    public Player Player => _player;

    public Transform PowerFirePoint => powerFirePoint;

    public PowerScriptableObject CurrentPower
    {
        get
        {
            var count = powers.Value.Count;

            // Return null if the powers array is empty
            if (count == 0)
                return null;

            currentPowerIndex.Value %= count;

            if (currentPowerIndex < 0)
                currentPowerIndex.Value += count;

            return powers.Value[currentPowerIndex];
        }
    }

    public PowerToken CurrentPowerToken => CurrentPower != null
        ? _powerTokens.GetValueOrDefault(CurrentPower)
        : null;

    public IReadOnlyCollection<PowerScriptableObject> Powers => powers.Value;

    public int CurrentPowerIndex => currentPowerIndex;

    public bool IsChargingPower => _isChargingPower;

    public bool WasPowerJustUsed { get; private set; }

    public Vector3 PowerAimHitPoint => _isPowerAimHitting
        ? _powerAimHit.point
        : Player.PlayerController.CameraPivot.transform.position +
          Player.PlayerController.CameraPivot.transform.forward * 1000;

    private VisualEffect CurrentChargeVfx
    {
        get
        {
            // If the current power is null, return the fireball charge VFX
            if (CurrentPower == null)
                return fireballChargeVfx;

            return GetChargeVfx(CurrentPower);
        }
    }

    // TODO: UPDATE THIS TOO
    private VisualEffect[] AllChargeVfx => new[] { fireballChargeVfx, electricChargeVfx, healthHaloChargeVfx };

    #endregion

    #region Initialization Functions

    private void Awake()
    {
        // Set the instance to this
        Instance = this;

        InitializeComponents();

        // Initialize the power collections
        InitializePowerCollections();

        // If there are no powers in Powers, then add the starting powers
        if (powers.Value.Count == 0 && startingPowers.Value != null && startingPowers.Value.Count > 0)
        {
            foreach (var scriptableObject in startingPowers.Value)
            {
                if (scriptableObject == null)
                    continue;

                AddPower(scriptableObject, false);
            }
        }

        // Initialize the input
        InitializeInput();

        // Set the power audio source to permanent
        powerAudioSource.SetPermanent(true);
    }

    // Start is called before the first frame update
    private void Start()
    {
        // Initialize the events
        InitializeEvents();

        // Initialize the vignette token
        _powerChargeVignetteToken =
            PostProcessingVolumeController.Instance?.VignetteModule.Tokens.AddToken(0, -1, true);

        // Add this to the debug manager
        DebugManager.Instance.AddDebuggedObject(this);

        if (CurrentPowerToken?.IsCharging == true)
            _isChargingPower = true;
    }

    private void OnEnable()
    {
        // Register this with the input manager
        InputManager.Instance.Register(this);
    }

    private void OnDisable()
    {
        // Unregister this with the input manager
        InputManager.Instance.Unregister(this);

        // Remove this from the debug manager
        DebugManager.Instance.RemoveDebuggedObject(this);
    }

    private void InitializeComponents()
    {
        // Get the TestPlayer component
        _player = GetComponent<Player>();
    }

    private void InitializeEvents()
    {
        _player.PlayerInfo.onRelapseStart += OnRelapseStart;

        // Add the event for the power used
        OnPowerUsed += PlaySoundOnUse;
        OnPowerUsed += OnPowerJustUsedOnUse;
        OnPowerUsed += ChromaticAberrationOnPowerUsed;
    }

    private void ChromaticAberrationOnPowerUsed(PlayerPowerManager arg1, PowerToken arg2)
    {
        // Return if the current power token is not a drug
        if (arg2.PowerScriptableObject.PowerType != PowerType.Drug)
            return;

        // Get the dynamic chromatic aberration module
        var dynamicChromaticAberrationModule =
            PostProcessingVolumeController.Instance.ScreenVolume.ChromaticAberrationModule;

        // Add a power token
        dynamicChromaticAberrationModule.AddPowerToken(1, 1);
    }

    private void OnPowerJustUsedOnUse(PlayerPowerManager arg1, PowerToken arg2)
    {
        // Set the was power just used flag to true
        WasPowerJustUsed = true;
    }

    #region Input

    public void InitializeInput()
    {
        InputActions.Add(
            new InputData(InputManager.Instance.PControls.Player.Power, InputType.Performed, OnPowerPerformed)
        );
        InputActions.Add(
            new InputData(InputManager.Instance.PControls.Player.Power, InputType.Canceled, OnPowerCanceled)
        );

        InputActions.Add(
            new InputData(InputManager.Instance.PControls.Player.ChangePower, InputType.Performed, OnPowerChanged)
        );

        // Add the inputs for 1, 2, 3, 4
        InputActions.Add(
            new InputData(InputManager.Instance.PControls.Player.Power1, InputType.Performed, OnPower1)
        );
        InputActions.Add(
            new InputData(InputManager.Instance.PControls.Player.Power2, InputType.Performed, OnPower2)
        );
        InputActions.Add(
            new InputData(InputManager.Instance.PControls.Player.Power3, InputType.Performed, OnPower3)
        );
        InputActions.Add(
            new InputData(InputManager.Instance.PControls.Player.Power4, InputType.Performed, OnPower4)
        );
    }

    private void OnPower1(InputAction.CallbackContext obj)
    {
        // Set the current power to the first power
        ChangePower(0);
    }

    private void OnPower2(InputAction.CallbackContext obj)
    {
        // Set the current power to the first power
        ChangePower(1);
    }

    private void OnPower3(InputAction.CallbackContext obj)
    {
        // Set the current power to the first power
        ChangePower(2);
    }

    private void OnPower4(InputAction.CallbackContext obj)
    {
        // Set the current power to the first power
        ChangePower(3);
    }

    #endregion

    private void InitializePowerCollections()
    {
        _powerTokens = new Dictionary<PowerScriptableObject, PowerToken>();

        // For each power in the all powers array, create a power token
        foreach (var pso in allPowers.Value)
            CreatePowerToken(pso);

        // Update the power collections
        UpdatePowerCollections(powers.Value.ToArray());
    }

    private void OnRelapseStart(PlayerInfo obj)
    {
        // Force the power to stop charging
        StopCharge();
    }

    #endregion

    #region Input Functions

    private void OnPowerChanged(InputAction.CallbackContext obj)
    {
        // Get the float value of the change power input
        var changePowerValue = obj.ReadValue<float>();

        // Scroll up or down based on the input
        var direction = changePowerValue > 0 ? 1 : -1;

        // Set the current power index to the next power
        ChangePower(currentPowerIndex + direction);
    }

    public void ChangePower(int index)
    {
        // Return if the powers array is empty
        if (powers.Value.Count == 0)
            return;

        // Don't change the power if the current power is active
        if (CurrentPowerToken.IsActiveEffectOn)
            return;

        // Reset the charge if the power is charging
        if (_isChargingPower)
            StopCharge();

        currentPowerIndex.Value = (index) % powers.Value.Count;
        if (currentPowerIndex < 0)
            currentPowerIndex.Value += powers.Value.Count;
    }

    private void OnPowerPerformed(InputAction.CallbackContext obj)
    {
        // Return if the current power is null
        if (CurrentPower == null)
            return;

        // Return if the current power token is null
        if (CurrentPowerToken == null)
            return;

        // Return if the power is currently cooling down
        if (CurrentPowerToken.IsCoolingDown)
            return;

        // Return if the power is currently active
        if (CurrentPowerToken.IsActiveEffectOn)
            return;

        // Skip if the player is currently relapsing
        if (_player.PlayerInfo.IsRelapsing)
            return;

        // Set the is charging power flag to true
        _isChargingPower = true;

        // Call the current power's start charge method
        var startedChargingThisFrame = CurrentPowerToken.ChargePercentage == 0;
        CurrentPower.PowerLogic.StartCharge(this, CurrentPowerToken, startedChargingThisFrame);

        // Set the charging flag to true
        CurrentPowerToken.SetChargingFlag(true);

        // Play the power charge sound
        PlaySound(CurrentPower.ChargeStartSound);
    }

    private void OnPowerCanceled(InputAction.CallbackContext obj)
    {
        // return if the current power is null
        if (CurrentPower == null)
            return;

        var isChargeComplete = StopCharge();

        // Stop the power charge sound
        StopSound();

        // If the charge is complete
        if (isChargeComplete)
            UsePower();
    }

    #endregion

    #region Update Methods

    // Update is called once per frame
    private void Update()
    {
        // Update the charge
        UpdateCharge();

        // Update the active powers
        UpdateActivePowers();

        // Update the passive powers
        UpdatePassivePowers();

        // Update the cooldowns
        UpdateCooldowns();

        // Update the vignette token
        UpdateVignetteToken();

        // Update the gauntlet charge VFX
        UpdateGauntletChargeVFX();
    }

    private void UpdateCharge()
    {
        // Skip if the current power is null
        if (CurrentPower == null)
            return;

        // Return if the current power token is null
        if (CurrentPowerToken == null)
            return;

        // Skip if the power is not charging
        if (!_isChargingPower)
            return;

        // Update the charge duration
        CurrentPowerToken.ChargePowerDuration();

        // Call the current power's charge method
        CurrentPower.PowerLogic.Charge(this, CurrentPowerToken);
    }

    private void UpdateActivePowers()
    {
        foreach (var power in _powerTokens.Keys)
        {
            // Get the current power token
            var cToken = _powerTokens[power];

            // Skip if the power is not active
            if (!cToken.IsActiveEffectOn)
                continue;

            // Update the active duration
            cToken.ActivePowerDuration();

            // Call the current power's active method
            power.PowerLogic.UpdateActiveEffect(this, cToken);

            // If the active percentage is 1, set the active flag to false
            if (cToken.ActivePercentage >= 1)
            {
                cToken.SetActiveFlag(false);

                // Call the current power's end active method
                power.PowerLogic.EndActiveEffect(this, cToken);

                // Set the cooldown flag to true
                CurrentPowerToken.SetCooldownFlag(true);

                // Set the cooldown duration to 0
                CurrentPowerToken.SetCooldownDuration(0);
            }
        }
    }

    private void UpdatePassivePowers()
    {
        foreach (var power in _powerTokens.Keys)
        {
            // Get the current power token
            var cToken = _powerTokens[power];

            // Skip if the power's passive effect is not active
            if (!cToken.IsPassiveEffectOn)
                continue;

            // Update the passive duration
            cToken.PassivePowerDuration();

            // Call the current power's passive method
            power.PowerLogic.UpdatePassiveEffect(this, cToken);

            // If the passive percentage is 1, 
            if (cToken.PassivePercentage >= 1)
            {
                // set the passive flag to false
                cToken.SetPassiveFlag(false);

                // Call the current power's end passive method
                power.PowerLogic.EndPassiveEffect(this, cToken);
            }
        }
    }

    private void UpdateCooldowns()
    {
        foreach (var power in _powerTokens.Keys)
        {
            // Get the current power token
            var cToken = _powerTokens[power];

            // Skip if the power is not cooling down
            if (!cToken.IsCoolingDown)
                continue;

            // Skip if the power is active (Redundant, but here to be safe)
            if (cToken.IsActiveEffectOn)
                continue;

            // Update the cooldown
            cToken.CooldownPowerDuration();

            // If the cooldown percentage is 1, set the cooldown flag to false
            // TODO: Create an event or something for when the cooldown stops
            if (cToken.CooldownPercentage >= 1)
            {
                cToken.SetCooldownFlag(false);

                // Play the power ready sound
                SoundManager.Instance.PlaySfx(power.PowerReadySound);
            }
        }
    }

    /// <summary>
    /// A method to update the power collections properly
    /// </summary>
    private void UpdatePowerCollections(params PowerScriptableObject[] newPowers)
    {
        // Condense the powers array by trimming the null values at the end
        int removeAmount;
        for (removeAmount = 0; removeAmount < powers.Value.Count; removeAmount++)
        {
            if (powers.Value[powers.Value.Count - 1 - removeAmount] != null)
                break;
        }

        // Remove the null values from the end of the array if removeAmount is greater than 0
        if (removeAmount > 0)
            PowerResize(powers.Value.Count - removeAmount);

        // Loop through all the new powers
        foreach (var power in newPowers)
        {
            if (power == null)
                continue;

            // Add the power to the power usage tokens
            CreatePowerToken(power);
        }

        // clamp the current power index to the new powers array
        currentPowerIndex.Value = Mathf.Clamp(currentPowerIndex, 0, powers.Value.Count - 1);

        // Skip if the current power is already set or if there are no powers
        if (CurrentPower != null || powers.Value.Count == 0)
            return;

        // Set the current power to the first power in the array
        currentPowerIndex.Value = 0;
    }

    private void CreatePowerToken(PowerScriptableObject power)
    {
        // Return if the power is null
        if (power == null)
            return;

        // Return if there is already a power token
        if (_powerTokens.ContainsKey(power))
            return;

        _powerTokens.Add(power, new PowerToken(power));
        powerTokensSo.value.Add(_powerTokens[power]);
    }

    private void UpdateVignetteToken()
    {
        var targetValue = 0f;

        // If the player's power is currently fully charged, set the target value to the charged vignette strength
        if (CurrentPowerToken != null && CurrentPowerToken.ChargePercentage >= 1)
            targetValue = chargedVignetteStrength;

        // Sine wave with a y from 0 to 1
        var sineWave = Mathf.Sin(chargedVignetteFlashesPerSecond * Time.time * Mathf.PI * 2 + Mathf.PI / 2) / 2 + 0.5f;

        // Lerp the vignette value to the target value
        if (_powerChargeVignetteToken != null)
        {
            _powerChargeVignetteToken.Value = Mathf.Lerp(
                _powerChargeVignetteToken.Value * sineWave,
                targetValue,
                CustomFunctions.FrameAmount(chargedVignetteLerpAmount)
            );
        }
    }

    private void UpdateGauntletChargeVFX()
    {
        // Make a hash set for the power vfx that have already been processed
        var processedVfx = new HashSet<VisualEffect>();

        // Process the current power's charge VFX first
        UpdateGauntletChargeVFXHelper(CurrentPower, CurrentPowerToken, CurrentChargeVfx, processedVfx);

        // For each power the player has equipped
        foreach (var power in powers.Value)
        {
            // Continue if the power is the current power
            if (power == CurrentPower)
                continue;

            var pToken = GetPowerToken(power);

            // Process the power's charge VFX
            UpdateGauntletChargeVFXHelper(power, pToken, GetChargeVfx(power), processedVfx);
        }
    }

    private void UpdateGauntletChargeVFXHelper(
        PowerScriptableObject power,
        PowerToken pToken,
        VisualEffect chargeVfx,
        HashSet<VisualEffect> processedVfx
    )
    {
        // Return if the power is null
        if (power == null)
            return;

        // If the vfx has already been processed, return
        if (processedVfx.Contains(chargeVfx))
            return;

        // Try to add the vfx to the fade times dictionary
        _fadeTimes.TryAdd(chargeVfx, 0);

        uint chargeState = 0;

        var isCurrentPower = power != null && power == CurrentPower;

        if (_isChargingPower && isCurrentPower && pToken != null)
        {
            // If the player is currently charging power, but it is not fully complete, set the charge state to 1
            if (pToken.ChargePercentage < 1)
                chargeState = 1;

            // If the player's power is currently fully charged, set the charge state to 2
            else
                chargeState = 2;
        }

        // Set the "ChargeState" uint property of the VFX graph
        chargeVfx.SetUInt("ChargeState", chargeState);

        if (isCurrentPower && (IsChargingPower || pToken?.ChargePercentage >= 1))
            _fadeTimes[chargeVfx] = Mathf.Clamp(_fadeTimes[chargeVfx] + Time.deltaTime, 0, MAX_FADE_TIME);
        else
            _fadeTimes[chargeVfx] = Mathf.Clamp(_fadeTimes[chargeVfx] - Time.deltaTime, 0, MAX_FADE_TIME);

        // Set the FadeTime float of the VFX graph for all charge effects
        chargeVfx.SetFloat("FadeTime", Mathf.InverseLerp(0, MAX_FADE_TIME, _fadeTimes[chargeVfx]));
        // chargeVfx.SetFloat("FadeTime", 1);

        // Add the VFX to the processed VFX hash set
        processedVfx.Add(chargeVfx);
    }

    private void LateUpdate()
    {
        // Reset the was power just used flag
        WasPowerJustUsed = false;
    }

    private void FixedUpdate()
    {
        // Update the power aim hit
        UpdatePowerAimHit();
    }

    private void UpdatePowerAimHit()
    {
        _isPowerAimHitting = Physics.Raycast(
            _player.PlayerController.CameraPivot.transform.position,
            _player.PlayerController.CameraPivot.transform.forward,
            out var hit,
            Mathf.Infinity,
            ~powerAimIgnoreLayers
        );

        // Fire a raycast from the camera pivot to the camera forward
        if (_isPowerAimHitting)
            _powerAimHit = hit;
    }

    #endregion

    private bool StopCharge()
    {
        // Set the is charging power flag to false
        _isChargingPower = false;

        // Return if the current power token is null
        if (CurrentPowerToken == null)
            return false;

        // Call the current power's release method
        var isChargeComplete = CurrentPowerToken.ChargePercentage >= 1;
        CurrentPower.PowerLogic.Release(this, CurrentPowerToken, isChargeComplete);

        // Set the charging flag to false
        CurrentPowerToken.SetChargingFlag(false);

        // Reset the charge duration if the power is not charging
        CurrentPowerToken.ResetChargeDuration();

        return isChargeComplete;
    }

    private void UsePower()
    {
        // Skip if the player is currently relapsing
        if (_player.PlayerInfo.IsRelapsing)
            return;

        // use the power
        CurrentPower.PowerLogic.Use(this, CurrentPowerToken);

        // Set the active flag to true
        CurrentPowerToken.SetActiveFlag(true);

        // Reset the active duration
        CurrentPowerToken.ResetActiveDuration();

        // Set the passive flag to true
        CurrentPowerToken.SetPassiveFlag(true);

        // Reset the passive duration
        CurrentPowerToken.ResetPassiveDuration();

        // After using the power, reset the charge duration
        CurrentPowerToken.ResetChargeDuration();

        // Start the active effect
        CurrentPower.PowerLogic.StartActiveEffect(this, CurrentPowerToken);

        // Start the passive effect
        CurrentPower.PowerLogic.StartPassiveEffect(this, CurrentPowerToken);

        // Change the player's tolerance
        _player.PlayerInfo.ChangeToxicity(CurrentPowerToken.ToleranceMeterImpact);

        // Invoke the event for the power used
        OnPowerUsed?.Invoke(this, CurrentPowerToken);
    }

    public void ResetPlayer()
    {
        // Reset each power token
        foreach (var powerToken in _powerTokens.Values)
            powerToken.Reset();

        // Set the is charging power flag to false
        _isChargingPower = false;
    }

    private VisualEffect GetChargeVfx(PowerScriptableObject power)
    {
        // Return null if the power is null
        if (power == null)
            return fireballChargeVfx;

        return power.ChargeVfxType switch
        {
            PowerVfxType.Fireball => fireballChargeVfx,
            PowerVfxType.GreenFireball => greenFireballChargeVfx,
            PowerVfxType.Electric => electricChargeVfx,
            PowerVfxType.HealthHalo => healthHaloChargeVfx,
            _ => null
        };
    }

    #region Event Functions

    private static void PlaySoundOnUse(PlayerPowerManager powerManager, PowerToken powerToken)
    {
        // // Kill the current sound
        // powerManager.powerAudioSource.Stop();

        // Play the power use sound
        powerManager.PlaySound(powerToken.PowerScriptableObject.PowerUseSound);
    }

    #endregion

    #region Public Methods

    public void AddPower(PowerScriptableObject powerScriptableObject, bool toolTip = true)
    {
        // Return if the power is null
        if (powerScriptableObject == null)
            return;

        // Check if the power is already in the array
        if (powers.Value.Contains(powerScriptableObject))
            return;

        // Add the power to the end of the powers array
        // Array.Resize(ref powers, powers.Value.Count + 1);
        PowerResize(powers.Value.Count + 1);
        powers.Value[^1] = powerScriptableObject;

        // Update the power collections
        UpdatePowerCollections(powerScriptableObject);

        var powerTypeName = powerScriptableObject.PowerType switch
        {
            PowerType.Drug => "Neuro",
            PowerType.Medicine => "Vital",
            _ => throw new ArgumentOutOfRangeException()
        };

        // Display a tooltip for the power
        if (toolTip)
            JournalTooltipManager.Instance.AddTooltip(
                $"New {powerTypeName}: {powerScriptableObject.PowerName}"
            );
    }

    private void AddPower(PowerToken powerToken)
    {
        // Add the power to the power tokens / replace the power token if it already exists
        _powerTokens[powerToken.PowerScriptableObject] = powerToken;

        // add the power to the end of the powers array
        if (!powers.Value.Contains(powerToken.PowerScriptableObject))
        {
            // Array.Resize(ref powers, powers.Value.Count + 1);
            PowerResize(powers.Value.Count + 1);
            powers.Value[^1] = powerToken.PowerScriptableObject;
        }

        // Update the power collections
        UpdatePowerCollections(powerToken.PowerScriptableObject);
    }

    private void PowerResize(int newSize)
    {
        // // Copy the original powers array
        // var powersCopy = new PowerScriptableObject[powers.Value.Count];
        // Array.Copy(powers.Value, powersCopy, powers.Value.Count);
        //
        // // Replace the powers array with a new empty array
        // powers.Value = new PowerScriptableObject[newSize];
        //
        // // Copy the original powers back to the new array
        // Array.Copy(powersCopy, powers.Value, powersCopy.Length);

        // Ensure the list does not contain any null values
        while (powers.Value.Count < newSize)
            powers.Value.Add(null);

        while (powers.Value.Count > newSize && powers.Value[^1] == null)
            powers.Value.RemoveAt(powers.Value.Count - 1);
    }

    public void RemovePower(PowerScriptableObject powerScriptableObject)
    {
        var isCurrentPower = CurrentPower == powerScriptableObject;
        var isLastPower = powers.Value.Count == 1;

        // Return if the player does not have the power
        if (!powers.Value.Contains(powerScriptableObject))
            return;

        // Remove the power from the powers array
        for (int i = 0; i < powers.Value.Count; i++)
        {
            // Look for the power in the array
            if (powers.Value[i] != powerScriptableObject)
                continue;

            // Remove the power from the array by shifting all the elements to the left
            for (int j = i; j < powers.Value.Count - 1; j++)
                powers.Value[j] = powers.Value[j + 1];

            // Resize the array
            // Array.Resize(ref powers, powers.Value.Count - 1);
            PowerResize(powers.Value.Count - 1);

            break;
        }

        // Remove the associated power token
        _powerTokens.Remove(powerScriptableObject);

        // Remove the power from the power tokens scriptable object
        powerTokensSo.value.Remove(powerTokensSo.value.Find(n => n.PowerScriptableObject == powerScriptableObject));

        // Update the power collections
        UpdatePowerCollections();

        if (isCurrentPower)
        {
            currentPowerIndex.Value = Mathf.Clamp(currentPowerIndex, 0, powers.Value.Count - 1);
            ChangePower(currentPowerIndex);
        }

        if (isLastPower)
            ClearPowers();
    }

    public void ClearPowers()
    {
        // Clear the power tokens, the drugs set, the meds set, and the powers array
        _powerTokens.Clear();
        // powers.Value = Array.Empty<PowerScriptableObject>();
        powers.Value.Clear();

        powerTokensSo.value.Clear();
    }

    public bool HasPower(PowerScriptableObject powerScriptableObject)
    {
        return powers.Value.Contains(powerScriptableObject);
    }

    public PowerToken GetPowerToken(PowerScriptableObject powerScriptableObject)
    {
        if (powerScriptableObject == null)
            return null;

        return _powerTokens.GetValueOrDefault(powerScriptableObject);
    }

    public void SetPowerLevel(PowerScriptableObject powerScriptableObject, int level)
    {
        // Get the power token
        var powerToken = GetPowerToken(powerScriptableObject);

        // Return if the power token is null
        if (powerToken == null)
            return;

        // Set the power level
        powerToken.SetPowerLevel(level);
    }

    public void PlaySound(Sound sound)
    {
        // Return if the sound is null
        if (sound == null)
            return;

        // Return if the audio source is null
        if (powerAudioSource == null)
            return;

        // Set the audio source to permanent
        powerAudioSource.SetPermanent(true);

        // Play the sound on the audio source
        powerAudioSource.Play(sound);
    }

    public void StopSound()
    {
        // Return if the audio source is null
        if (powerAudioSource == null)
            return;

        // Stop the audio source
        powerAudioSource.Stop();
    }

    public PowerScriptableObject GetPowerAtIndex(int index)
    {
        return index >= 0 && index < powers.Value.Count ? powers.Value[index] : null;
    }

    public void SetPowerAtIndex(PowerScriptableObject power, int index)
    {
        if (index < 0 || index >= powers.Value.Count)
            return;

        // Create an array that represents the new powers array
        var newPowers = new PowerScriptableObject[powers.Value.Count];

        // Populate the array with the current powers
        for (int i = 0; i < powers.Value.Count; i++)
            newPowers[i] = powers.Value[i];

        // Replace the power at the index
        newPowers[index] = power;

        // Clear the powers array
        ClearPowers();

        // Add back the powers
        foreach (var cPower in newPowers)
            AddPower(cPower, false);
    }

    public string GetDebugText()
    {
        if (CurrentPower == null)
            return "No Power Selected!\n";

        StringBuilder debugString = new();

        float tolerancePercentage;
        if (_player.PlayerInfo.MaxToxicity == 0)
            tolerancePercentage = 0;
        else
            tolerancePercentage = _player.PlayerInfo.CurrentToxicity / _player.PlayerInfo.MaxToxicity * 100;

        debugString.Append(
            $"Toxicity: {_player.PlayerInfo.CurrentToxicity:0.00} / {_player.PlayerInfo.MaxToxicity:0.00} ({tolerancePercentage:0.00}%)\n\n");

        debugString.Append($"Current Power: {CurrentPower.name}\n");
        // debugString.Append($"\tPurity (Level): {CurrentPowerToken.CurrentLevel}\n");
        debugString.Append($"\nToxicity Impact: {CurrentPowerToken.ToleranceMeterImpact}\n");

        // Charging Logic
        debugString.Append($"\tIs Charging? {CurrentPowerToken.IsCharging}\n");

        if (CurrentPowerToken.IsCharging)
        {
            debugString.Append($"\t\tCharge: {CurrentPowerToken.ChargePercentage * 100:0.00}%\n");
            debugString.Append(
                $"\t\tDuration: {CurrentPowerToken.CurrentChargeDuration:0.00}s / {CurrentPower.ChargeDuration:0.00}s\n");
        }

        // Active Logic
        debugString.Append($"\tActive Effect? {CurrentPowerToken.IsActiveEffectOn}\n");

        if (CurrentPowerToken.IsActiveEffectOn)
        {
            debugString.Append($"\t\tOn: {CurrentPowerToken.ActivePercentage * 100:0.00}%\n");
            debugString.Append(
                $"\t\tDuration: {CurrentPowerToken.CurrentActiveDuration:0.00}s / {CurrentPower.ActiveEffectDuration:0.00}s\n");
        }

        // Passive Logic
        debugString.Append($"\tPassive Effect? {CurrentPowerToken.IsActiveEffectOn}\n");

        if (CurrentPowerToken.IsPassiveEffectOn)
        {
            debugString.Append($"\t\tOn: {CurrentPowerToken.PassivePercentage * 100:0.00}%\n");
            debugString.Append(
                $"\t\tDuration: {CurrentPowerToken.CurrentPassiveDuration:0.00}s / {CurrentPower.PassiveEffectDuration:0.00}s\n");
        }

        // Cooldown Logic
        debugString.Append($"\tIs Cooling Down? {CurrentPowerToken.IsCoolingDown}\n");

        if (CurrentPowerToken.IsCoolingDown)
        {
            debugString.Append($"\t\tCooldown: {CurrentPowerToken.CooldownPercentage * 100:0.00}%\n");
            debugString.Append(
                $"\t\tDuration: {CurrentPowerToken.CurrentCooldownDuration:0.00}s / {CurrentPower.Cooldown:0.00}s\n");
        }

        return debugString.ToString();
    }

    #endregion

    #region IPlayerLoaderInfo

    public GameObject GameObject => gameObject;
    public string Id => "PlayerPowerManager";

    private const string POWER_LEVEL_KEY = "_powerLevel";
    private const string ACTIVE_DURATION_KEY = "_activeDuration";
    private const string PASSIVE_DURATION_KEY = "_passiveDuration";
    private const string COOLDOWN_DURATION_KEY = "_cooldownDuration";

    public void LoadData(PlayerLoader playerLoader, bool restore)
    {
        // End the passive and active effects of all the powers
        foreach (var power in _powerTokens.Keys)
        {
            var powerToken = _powerTokens[power];

            // End the active effect
            if (powerToken.IsActiveEffectOn)
                power.PowerLogic.EndActiveEffect(this, powerToken);

            // End the passive effect
            if (powerToken.IsPassiveEffectOn)
                power.PowerLogic.EndPassiveEffect(this, powerToken);
        }

        // Clear the power tokens, the drugs set, the meds set, and the powers array
        ClearPowers();

        // Load the power scriptable objects
        foreach (var pso in PowerHelper.Instance.Powers)
        {
            // Check if the id is in the player loader's memory
            if (!playerLoader.TryGetDataFromMemory(Id, pso.UniqueId, out bool _))
                continue;

            // Load in each of the power token's stats
            if (!playerLoader.TryGetDataFromMemory(Id, $"{pso.UniqueId}{POWER_LEVEL_KEY}", out int powerLevel))
                continue;

            if (!playerLoader.TryGetDataFromMemory(Id, $"{pso.UniqueId}{ACTIVE_DURATION_KEY}",
                    out float activeDuration))
                continue;

            if (!playerLoader.TryGetDataFromMemory(Id, $"{pso.UniqueId}{PASSIVE_DURATION_KEY}",
                    out float passiveDuration))
                continue;

            if (!playerLoader.TryGetDataFromMemory(Id, $"{pso.UniqueId}{COOLDOWN_DURATION_KEY}",
                    out float cooldownDuration))
                continue;

            // Create a new power token
            var powerToken =
                PowerToken.CreatePowerToken(pso, powerLevel, activeDuration, passiveDuration, cooldownDuration);

            // Add the power token
            AddPower(powerToken);

            // Add the power token to the scriptable object
            powerTokensSo.value.Add(powerToken);
        }
    }

    public void SaveData(PlayerLoader playerLoader)
    {
        // For each power, save the data
        foreach (var power in _powerTokens.Keys)
        {
            var powerToken = _powerTokens[power];

            // Save the id of the power
            var powerIdData = new DataInfo($"{power.UniqueId}", true);
            playerLoader.AddDataToMemory(Id, powerIdData);

            // Save the power level
            var powerLevelData = new DataInfo($"{power.UniqueId}{POWER_LEVEL_KEY}", powerToken.CurrentLevel);
            playerLoader.AddDataToMemory(Id, powerLevelData);

            // Save the power active duration
            var activeDurationData =
                new DataInfo($"{power.UniqueId}{ACTIVE_DURATION_KEY}", powerToken.CurrentActiveDuration);
            playerLoader.AddDataToMemory(Id, activeDurationData);

            // Save the power passive duration
            var passiveDurationData =
                new DataInfo($"{power.UniqueId}{PASSIVE_DURATION_KEY}", powerToken.CurrentPassiveDuration);
            playerLoader.AddDataToMemory(Id, passiveDurationData);

            // Save the power cooldown duration
            var cooldownDurationData =
                new DataInfo($"{power.UniqueId}{COOLDOWN_DURATION_KEY}", powerToken.CurrentCooldownDuration);
            playerLoader.AddDataToMemory(Id, cooldownDurationData);
        }
    }

    #endregion
}