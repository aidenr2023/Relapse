using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DebugManagerHelper : MonoBehaviour, IDamager, IUsesInput, IDebugged
{
    public static DebugManagerHelper Instance { get; private set; }

    #region Serialized Fields

    [Tooltip("A Canvas object to display debug text")] [SerializeField]
    private Canvas debugCanvas;

    [Tooltip("A TMP_Text object to display debug text")] [SerializeField]
    private TMP_Text debugText;

    [SerializeField] [Range(0, 1)] private float healthMult = 1f;
    [SerializeField] [Min(0)] private float toleranceMult = 10f;

    [SerializeField] private Material interactableMaterial;

    [Header("Debug Scene Skips")] [SerializeField]
    private GenericGun levelSkipGun;

    [SerializeField] private DebugSceneLevelInfo[] debugSceneLevelInfos;

    [SerializeField, Min(0)] private float textUpdateRate = 1 / 10f;

    #endregion

    #region Private Fields

    private float _healthChange;
    private float _toleranceChange;

    #endregion

    #region Getters

    public GameObject GameObject => gameObject;

    public HashSet<InputData> InputActions { get; } = new();

    private Player Player => Player.Instance;


    public Sound NormalHitSfx => null;
    public Sound CriticalHitSfx => null;

    #endregion

    #region Initialization Functions

    private void Awake()
    {
        // If the instance is not null and not this, destroy this
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Set the instance
        Instance = this;

        // Initialize the input
        InitializeInput();

        // Add this to the debug manager
        DebugManager.Instance.AddDebuggedObject(this);
    }

    private void OnDestroy()
    {
        // If the instance is this, set the instance to null
        if (Instance == this)
            Instance = null;

        // Remove this from the debug manager
        DebugManager.Instance.RemoveDebuggedObject(this);
    }

    // Start is called before the first frame update
    private void Start()
    {
        // Set the visibility of the debug text
        SetDebugVisibility(DebugManager.Instance.IsDebugMode);

        // Start the text routine
        StartCoroutine(SetTextRoutine());
    }

    private void OnEnable()
    {
        // Register this to the input manager
        InputManager.Instance.Register(this);
    }

    private void OnDisable()
    {
        // Unregister this from the input manager
        InputManager.Instance.Unregister(this);
    }

    public void InitializeInput()
    {
        InputActions.Add(
            new InputData(InputManager.Instance.OtherControls.Debug.ToggleDebug, InputType.Performed, ToggleDebugMode)
        );

        InputActions.Add(new InputData(
            InputManager.Instance.OtherControls.Debug.DebugHealth, InputType.Performed, OnDebugHealthPerformed)
        );
        InputActions.Add(new InputData(
            InputManager.Instance.OtherControls.Debug.DebugHealth, InputType.Canceled, OnDebugHealthCanceled)
        );

        InputActions.Add(new InputData(
            InputManager.Instance.OtherControls.Debug.DebugTolerance, InputType.Performed, OnDebugTolerancePerformed)
        );
        InputActions.Add(new InputData(
            InputManager.Instance.OtherControls.Debug.DebugTolerance, InputType.Canceled, OnDebugToleranceCanceled)
        );
    }

    #region Input Functions

    private void ToggleDebugMode(InputAction.CallbackContext ctx)
    {
        // Toggle the debug mode
        DebugManager.Instance.IsDebugMode = !DebugManager.Instance.IsDebugMode;
    }


    private void OnDebugHealthPerformed(InputAction.CallbackContext context)
    {
        _healthChange = context.ReadValue<float>();
    }

    private void OnDebugHealthCanceled(InputAction.CallbackContext context)
    {
        _healthChange = 0;
    }

    private void OnDebugTolerancePerformed(InputAction.CallbackContext context)
    {
        _toleranceChange = context.ReadValue<float>();
    }

    private void OnDebugToleranceCanceled(InputAction.CallbackContext context)
    {
        _toleranceChange = 0;
    }

    #endregion

    #endregion

    private bool _isUIVisible = true;

    // Update is called once per frame
    private void Update()
    {
        // Set the debug text visibility
        SetDebugVisibility(DebugManager.Instance.IsDebugMode);

        // Update the tolerance and health
        UpdateToleranceAndHealth();

        if (DebugManager.Instance.IsDebugMode)
        {
            // Scene Skips
            if (Input.GetKeyDown(KeyCode.Alpha1))
                DebugLoadScene(debugSceneLevelInfos[0]);
            if (Input.GetKeyDown(KeyCode.Alpha2))
                DebugLoadScene(debugSceneLevelInfos[1]);
            if (Input.GetKeyDown(KeyCode.Alpha3))
                DebugLoadScene(debugSceneLevelInfos[2]);
            if (Input.GetKeyDown(KeyCode.Alpha4))
                DebugLoadScene(debugSceneLevelInfos[3]);
            if (Input.GetKeyDown(KeyCode.Alpha5))
                DebugLoadScene(debugSceneLevelInfos[4]);
            if (Input.GetKeyDown(KeyCode.Alpha6))
                DebugLoadScene(debugSceneLevelInfos[5]);
            if (Input.GetKeyDown(KeyCode.Alpha7))
                DebugLoadScene(debugSceneLevelInfos[6]);
            if (Input.GetKeyDown(KeyCode.Alpha8))
                DebugLoadScene(debugSceneLevelInfos[7]);
            if (Input.GetKeyDown(KeyCode.Alpha9))
                DebugLoadScene(debugSceneLevelInfos[8]);
            if (Input.GetKeyDown(KeyCode.Alpha0))
                DebugLoadScene(debugSceneLevelInfos[9]);

            // Power Changes
            if (Input.GetKeyDown(KeyCode.Keypad0))
                ChangePower(0);
            if (Input.GetKeyDown(KeyCode.Keypad1))
                ChangePower(1);
            if (Input.GetKeyDown(KeyCode.Keypad2))
                ChangePower(2);
            if (Input.GetKeyDown(KeyCode.Keypad3))
                ChangePower(3);

            // Breakpoint
            if (Input.GetKeyDown(KeyCode.B))
                Debug.Break();

            if (Input.GetKeyDown(KeyCode.Z))
                FindBadInteractableMaterials();

            // Set money to 999999
            if (Input.GetKeyDown(KeyCode.Keypad9))
            {
                var inventoryVar = Player.PlayerInventory.InventoryVariable;
                inventoryVar.AddItem(inventoryVar.MoneyObject, 999999 - inventoryVar.MoneyCount);
            }
        }

        // Toggle the UI
        if (Input.GetKeyDown(KeyCode.X))
            ToggleUI();

        // Decrease the health
        if (Input.GetKeyDown(KeyCode.K))
            Player.PlayerInfo.ChangeHealth(-10, Player.PlayerInfo, Player.PlayerInfo, Player.transform.position);

        if (Input.GetKeyDown(KeyCode.Y))
        {
            Player.PlayerInfo.ChangeHealth(
                -Player.PlayerInfo.MaxHealth,
                Player.PlayerInfo, this,
                Player.transform.position
            );
        }

        return;

        IEnumerator TestCoroutine(string text, float waitTime)
        {
            Debug.Log($"{text} - [{waitTime:0.00}]");
            yield return new WaitForSeconds(waitTime);
        }
    }

    private void ToggleUI()
    {
        const float transitionTime = 0.0625f;
        
        if (_isUIVisible && DebugManager.Instance.IsDebugMode)
        {
            GameUIHelper.Instance.AddUIHider(this, transitionTime);
            _isUIVisible = false;
        }
        else
        {
            GameUIHelper.Instance.RemoveUIHider(this, transitionTime);
            _isUIVisible = true;
        }
    }

    private void DebugLoadScene(LevelSectionSceneInfo[] levelInfo)
    {
        // If the array is empty or null, return
        if (levelInfo == null || levelInfo.Length == 0)
            return;

        // If there is no player, return
        if (Player == null)
            return;

        // Move the player back to their original scene
        Player.transform.parent = Player.OriginalSceneObject.transform;

        // Find the scene the player is in
        var playerSceneField = (SceneField)Player.gameObject.scene.name;

        // Get the currently managed scenes from the AsyncSceneManager
        var managedScenes = AsyncSceneManager.Instance.GetManagedScenes();

        // Create a level section scene info array with all the managed scenes EXCEPT the player's scene
        var scenesToUnload = new List<string>();
        foreach (var scene in managedScenes)
        {
            if (scene == playerSceneField.SceneName)
                continue;

            scenesToUnload.Add(scene);

            Debug.Log($"Unload: {scene}");
        }

        // Convert the scenes to unload to a LevelSectionSceneInfo array
        var scenesToUnloadInfo = scenesToUnload.Select(scene => LevelSectionSceneInfo.Create(null, scene)).ToArray();

        // Create a new SceneLoaderInformation based on the input
        var sceneLoaderInformation = SceneLoaderInformation.Create(levelInfo, scenesToUnloadInfo);

        // Load the scene
        AsyncSceneManager.Instance.DebugLoadSceneSynchronous(sceneLoaderInformation);

        // Set the parent of the player back to null
        Player.transform.parent = null;
    }

    private void DebugLoadScene(DebugSceneLevelInfo sceneLevelInfo)
    {
        // If the scene level info is null, return
        DebugLoadScene(sceneLevelInfo.LevelInfo);

        // If the powers are null, return
        if (sceneLevelInfo.Powers == null)
            return;

        var player = Player;

        // Clear the player's powers and add the new powers
        var powers = player.PlayerPowerManager.Powers;

        player.PlayerPowerManager.ClearPowers();

        foreach (var power in sceneLevelInfo.Powers)
            player.PlayerPowerManager.AddPower(power, false);

        // If the player has no gun, add the level skip gun
        if (player.WeaponManager.EquippedGun == null && levelSkipGun != null)
        {
            // Instantiate the gun
            var gun = Instantiate(levelSkipGun);
            player.WeaponManager.EquipGun(gun);
        }
    }

    private void FindBadInteractableMaterials()
    {
        // Return if the interactable material is null
        if (interactableMaterial == null)
            return;

        // Look through the entire scene for any interactables
        var interactables = FindObjectsOfType<MonoBehaviour>()
            .OfType<IInteractable>();

        foreach (var interactable in interactables)
        {
            // Get all the renderers in the interactable
            var renderers = interactable.GameObject.GetComponentsInChildren<Renderer>();

            foreach (var cRenderer in renderers)
            {
                // Get the materials in the renderer
                var materials = cRenderer.materials;

                // If the renderer contains a material w/ the same shader as the interactable material
                if (materials.All(material => material.shader != interactableMaterial.shader))
                    continue;

                // Snitch
                Debug.Log($"BAD INTERACTABLE MATERIAL: {cRenderer.gameObject} ({interactable.GameObject})", cRenderer);
            }
        }
    }

    private void ChangePower(int equippedPowerIndex)
    {
        var pso = PowerHelper.Instance.Powers;

        var allPowers = new List<PowerScriptableObject>(pso);

        // get the power at the given index
        var indexedPower = Player.PlayerPowerManager.GetPowerAtIndex(equippedPowerIndex);

        // For each of the player's currently equipped powers,
        // remove them from the allPowersHashSet
        foreach (var power in Player.PlayerPowerManager.Powers)
        {
            // Skip if the power is the indexed power
            if (power == indexedPower)
                continue;

            allPowers.Remove(power);
        }

        // If the indexed power is null, just add the first power
        if (indexedPower == null)
        {
            Player.PlayerPowerManager.AddPower(allPowers[0]);
            return;
        }

        // Store the current Power index
        var currentPowerIndex = Player.PlayerPowerManager.CurrentPowerIndex;

        var nextIndex = -1;

        if (indexedPower != null)
            nextIndex = allPowers.IndexOf(indexedPower);

        nextIndex++;

        var nextPower = allPowers[nextIndex % allPowers.Count];

        // Set the power at that powerIndex to the nextPower
        Player.PlayerPowerManager.SetPowerAtIndex(nextPower, equippedPowerIndex);

        // Set the current power index to the current power index
        Player.PlayerPowerManager.ChangePower(currentPowerIndex);
    }

    private void UpdateToleranceAndHealth()
    {
        // If there is no player, return
        if (Player == null)
            return;

        var playerInfo = Player.PlayerInfo;

        playerInfo.ChangeHealth(
            _healthChange * Time.unscaledDeltaTime *
            healthMult * playerInfo.MaxHealth,
            playerInfo, this, Player.transform.position
        );

        if (_toleranceChange < 0 && playerInfo.IsRelapsing)
            playerInfo.ChangeToxicity(-playerInfo.CurrentToxicity);
        else
            playerInfo.ChangeToxicity(_toleranceChange * Time.unscaledDeltaTime * toleranceMult);
    }

    private string UpdateText()
    {
        // Return if not in debug mode
        if (!DebugManager.Instance.IsDebugMode)
            return "";

        // If the debug text is null, return
        if (debugText == null)
            return "";

        // Create a new string from the debug managed objects
        StringBuilder textString = new();

        var debuggedObjects = DebugManager.Instance.DebuggedObjects.ToArray();
        foreach (var obj in debuggedObjects)
        {
            textString.Append(obj.GetDebugText());
            textString.Append('\n');
        }

        // Set the text
        return textString.ToString();
    }

    private IEnumerator SetTextRoutine()
    {
        while (true)
        {
            // Set the text if in debug mode
            if (DebugManager.Instance.IsDebugMode)
                debugText.text = UpdateText();

            // Wait for the text update rate
            yield return new WaitForSecondsRealtime(textUpdateRate);
        }
    }

    private void SetDebugVisibility(bool isVisible)
    {
        // Set the debug canvas's visibility
        debugCanvas.enabled = isVisible;
    }

    public string GetDebugText()
    {
        var sb = new StringBuilder();

        sb.Append($"Current Movement Type: {AsyncSceneManager.Instance._movementType}");

        return sb.ToString();
    }

    [Serializable]
    private struct DebugSceneLevelInfo
    {
        [SerializeField] private LevelSectionSceneInfo[] levelInfo;
        [SerializeField] private PowerScriptableObject[] powers;

        public LevelSectionSceneInfo[] LevelInfo => levelInfo;
        public PowerScriptableObject[] Powers => powers;
    }
}