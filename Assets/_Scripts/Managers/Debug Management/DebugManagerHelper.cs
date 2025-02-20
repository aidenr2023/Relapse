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
    private LevelSectionSceneInfo[] levelInfo1;

    [SerializeField] private LevelSectionSceneInfo[] levelInfo2;
    [SerializeField] private LevelSectionSceneInfo[] levelInfo3;
    [SerializeField] private LevelSectionSceneInfo[] levelInfo4;
    [SerializeField] private LevelSectionSceneInfo[] levelInfo5;
    [SerializeField] private LevelSectionSceneInfo[] levelInfo6;
    [SerializeField] private LevelSectionSceneInfo[] levelInfo7;
    [SerializeField] private LevelSectionSceneInfo[] levelInfo8;
    [SerializeField] private LevelSectionSceneInfo[] levelInfo9;
    [SerializeField] private LevelSectionSceneInfo[] levelInfo10;

    #endregion

    #region Private Fields

    private float _healthChange;
    private float _toleranceChange;

    #endregion

    #region Getters

    public GameObject GameObject => gameObject;

    public HashSet<InputData> InputActions { get; } = new();

    private Player Player => Player.Instance;

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

        // Set the debug text visibility
        SetDebugVisibility(DebugManager.Instance.IsDebugMode);
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

    // Update is called once per frame
    private void Update()
    {
        // Update the text
        UpdateText();

        // Update the tolerance and health
        UpdateToleranceAndHealth();

        if (Input.GetKeyDown(KeyCode.Comma))
            UserSettings.Instance.SetGamma(UserSettings.Instance.Gamma - 0.1f);
        if (Input.GetKeyDown(KeyCode.Period))
            UserSettings.Instance.SetGamma(UserSettings.Instance.Gamma + 0.1f);
        if (Input.GetKeyDown(KeyCode.Slash))
            UserSettings.Instance.SetGamma(0);

        if (Input.GetKeyDown(KeyCode.Z))
            FindBadInteractableMaterials();

        if (DebugManager.Instance.IsDebugMode)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
                DebugLoadScene(levelInfo1);
            if (Input.GetKeyDown(KeyCode.Alpha2))
                DebugLoadScene(levelInfo2);
            if (Input.GetKeyDown(KeyCode.Alpha3))
                DebugLoadScene(levelInfo3);
            if (Input.GetKeyDown(KeyCode.Alpha4))
                DebugLoadScene(levelInfo4);
            if (Input.GetKeyDown(KeyCode.Alpha5))
                DebugLoadScene(levelInfo5);
            if (Input.GetKeyDown(KeyCode.Alpha6))
                DebugLoadScene(levelInfo6);
            if (Input.GetKeyDown(KeyCode.Alpha7))
                DebugLoadScene(levelInfo7);
            if (Input.GetKeyDown(KeyCode.Alpha8))
                DebugLoadScene(levelInfo8);
            if (Input.GetKeyDown(KeyCode.Alpha9))
                DebugLoadScene(levelInfo9);
            if (Input.GetKeyDown(KeyCode.Alpha0))
                DebugLoadScene(levelInfo10);
        }

    }

    private void DebugLoadScene(LevelSectionSceneInfo[] levelInfo)
    {
        // If the array is empty or null, return
        if (levelInfo == null || levelInfo.Length == 0)
            return;

        // If there is no player, return
        if (Player.Instance == null)
            return;
        
        // Move the player back to their original scene
        Player.Instance.transform.parent = Player.Instance.OriginalSceneObject.transform;

        // Find the scene the player is in
        var playerSceneField = (SceneField)Player.Instance.gameObject.scene.name;

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
        Player.Instance.transform.parent = null;
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

    private void UpdateToleranceAndHealth()
    {
        // If there is no player, return
        if (Player == null)
            return;

        Player.PlayerInfo.ChangeHealth(
            _healthChange * Time.unscaledDeltaTime *
            healthMult * Player.PlayerInfo.MaxHealth,
            Player.PlayerInfo, this, Player.transform.position
        );
        Player.PlayerInfo.ChangeTolerance(_toleranceChange * Time.unscaledDeltaTime * toleranceMult);
    }

    private void UpdateText()
    {
        // If the debug text is null, return
        if (debugText == null)
            return;

        // Create a new string from the debug managed objects
        StringBuilder textString = new();
        foreach (var obj in DebugManager.Instance.DebuggedObjects)
        {
            textString.Append(obj.GetDebugText());
            textString.Append('\n');
        }

        // Set the text
        debugText.text = textString.ToString();
    }

    private void SetDebugVisibility(bool isVisible)
    {
        // Set the debug canvas's visibility
        debugCanvas.enabled = isVisible;
    }

    public string GetDebugText()
    {
        var sb = new StringBuilder();

        // sb.Append($"Serialization Data Info:\n");
        //
        // foreach (var keyValue in LevelLoader.Instance.Data)
        // {
        //     var uniqueId = keyValue.Key;
        //
        //     sb.Append($"\tUniqueId: {uniqueId}\n");
        //
        //     foreach (var data in keyValue.Value)
        //         sb.Append($"\t\t{data.Key}: {data.Value}\n");
        // }

        return sb.ToString();
    }
}