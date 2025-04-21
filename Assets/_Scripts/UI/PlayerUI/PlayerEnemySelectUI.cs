using System;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup), typeof(DynamicMaterialManager))]
public class PlayerEnemySelectUI : MonoBehaviour
{
    private const float ALPHA_THRESHOLD = .001f;

    #region Serialized Fields

    [SerializeField] private GameObject enemySelectUI;
    [SerializeField, Range(0, 1)] private float iconOpacity = .5f;
    [SerializeField, Min(0)] private float lerpSpeed = 0.25f;

    [SerializeField, Range(0, 1)] private float minDistanceScale = 0.5f;
    [SerializeField, Min(0)] private float scaleStartDistance = 20f;

    // angles per second
    [SerializeField] private float rotationSpeed = 360f;

    [SerializeField] private float scaleBobAmount = 0.25f;
    [SerializeField] private float scaleBobFrequency = 1f;

    #endregion

    private CanvasGroup _canvasGroup;
    private DynamicMaterialManager _dynamicMaterialManager;

    private float _currentScaleBob;
    private Enemy _previousEnemy;

    public static PlayerEnemySelectUI Instance { get; private set; }

    private void Awake()
    {
        // Set the instance to this
        Instance = this;

        // Get the CanvasGroup component
        _canvasGroup = GetComponent<CanvasGroup>();

        // Set the opacity to 0
        _canvasGroup.alpha = 0;

        // Get the DynamicMaterialManager component
        _dynamicMaterialManager = GetComponent<DynamicMaterialManager>();
    }

    private void Start()
    {
        _dynamicMaterialManager.ChangeRenderers(null);
    }

    private void Update()
    {
        var isVisible = Player.Instance != null &&
                        Player.Instance.PlayerEnemySelect.SelectedEnemy.HasValue &&
                        Player.Instance.PlayerPowerManager.CurrentPower != null &&
                        Player.Instance.PlayerPowerManager.CurrentPower.UsesReticle;

        var hasEnemy = Player.Instance?.PlayerEnemySelect.SelectedEnemy.HasValue ?? false;

        // Update the opacity
        UpdateOpacity(isVisible, hasEnemy);

        // Update the position
        UpdatePosition(isVisible, hasEnemy);

        // Update the scale bob
        UpdateScaleBob(isVisible, hasEnemy);

        // Update the scale
        UpdateScale(isVisible, hasEnemy);

        // Update the rotation
        UpdateRotation(isVisible, hasEnemy);

        // // Update the Material Manager
        // UpdateMaterialManager(isVisible, hasEnemy);

        // Update the previous enemy
        // _previousEnemy = Player.Instance?.PlayerEnemySelect?.SelectedEnemy;
        _previousEnemy = Player.Instance?.PlayerEnemySelect.SelectedEnemy!.Switch();
    }

    private void UpdateMaterialManager(bool isVisible, bool hasEnemy)
    {
        if (!isVisible)
        {
            _dynamicMaterialManager.ChangeRenderers(null);
            return;
        }

        var currentEnemy = Player.Instance.PlayerEnemySelect.SelectedEnemy.Switch();

        var needsToChangeRenderers = isVisible && _previousEnemy != currentEnemy;

        if (!needsToChangeRenderers)
            return;

        Debug.Log($"Changing renderers from [{_previousEnemy}] to [{currentEnemy}]");

        // Remove the material from the previous enemy
        if (currentEnemy != null)
        {
            _dynamicMaterialManager.ChangeRenderers(currentEnemy.gameObject);
            _dynamicMaterialManager.AddMaterial();
        }

        // Set the material to the current enemy
        else if (_previousEnemy != null)
        {
            _dynamicMaterialManager.ChangeRenderers(null);
        }
    }

    private void UpdateOpacity(bool isVisible, bool hasEnemy)
    {
        var desiredOpacity = 0f;

        if (isVisible)
            desiredOpacity = iconOpacity;

        const float defaultFrameTime = 1 / 60f;
        var frameTime = Time.deltaTime / defaultFrameTime;

        // Lerp the opacity
        _canvasGroup.alpha = Mathf.Lerp(_canvasGroup.alpha, desiredOpacity, lerpSpeed * frameTime);

        // If the difference between the current opacity and the desired opacity is less than the threshold,
        // set the opacity to the desired opacity
        if (Mathf.Abs(_canvasGroup.alpha - desiredOpacity) < ALPHA_THRESHOLD)
            _canvasGroup.alpha = desiredOpacity;
    }

    private void UpdatePosition(bool isVisible, bool hasEnemy)
    {
        if (!hasEnemy)
            return;

        var enemyScreenPosition = Player.Instance.PlayerEnemySelect.EnemyScreenPosition;

        // Get the screen dimensions
        var screenDimensions = new Vector2(Screen.width, Screen.height);

        // Set the position of the enemy select UI
        enemySelectUI.transform.localPosition = enemyScreenPosition - screenDimensions / 2;
    }

    private void UpdateScaleBob(bool isVisible, bool hasEnemy)
    {
        _currentScaleBob = Mathf.Sin(Time.time * Mathf.PI * scaleBobFrequency) * scaleBobAmount;
    }

    private void UpdateScale(bool isVisible, bool hasEnemy)
    {
        if (!hasEnemy)
            return;

        if (Player.Instance == null)
            return;
        
        // Get the distance between the enemy and the player
        var distance = Vector3.Distance(
            Player.Instance.transform.position,
            Player.Instance.PlayerEnemySelect.EnemyPosition
        );

        // Reverse lerp the distance
        var inverseLerp =
            Mathf.InverseLerp(scaleStartDistance, Player.Instance.PlayerEnemySelect.MaxDistance, distance);

        var scale = Mathf.Lerp(minDistanceScale, 1, 1 - inverseLerp);

        // Set the scale of the enemy select UI
        enemySelectUI.transform.localScale = new Vector3(
            scale + _currentScaleBob,
            scale + _currentScaleBob,
            scale + _currentScaleBob
        );
    }

    private void UpdateRotation(bool isVisible, bool hasEnemy)
    {
        // Rotate the enemy select UI
        enemySelectUI.transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
    }
}