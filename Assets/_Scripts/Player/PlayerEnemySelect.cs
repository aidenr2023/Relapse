using System;
using System.Linq;
using UnityEngine;

public class PlayerEnemySelect : ComponentScript<Player>
{
    #region Serialized Fields

    [SerializeField] private CameraManagerReference cameraManager;

    [SerializeField] private Vector2 originalScreenSize = new Vector2(1920, 1080);
    [SerializeField] private float aimSquareSize = 400;
    [SerializeField] private float maxDistance = 100;

    [SerializeField] private LayerMask enemyLayerMask;

    [SerializeField] private bool showDebug;

    #endregion

    #region Private Fields

    private Vector3 _enemyPosition;

    #endregion

    #region Getters

    public Vector2 OriginalScreenSize => originalScreenSize;

    public float MaxDistance => maxDistance;

    private float ActualAimSquareSize => aimSquareSize * (Screen.width / originalScreenSize.x);

    public Option<Enemy> SelectedEnemy { get; private set; } = Option<Enemy>.None;

    public Vector3 EnemyPosition => _enemyPosition;

    public Vector2 EnemyScreenPosition { get; private set; }

    #endregion

    private void FixedUpdate()
    {
        // Get the main camera
        var mainCam = cameraManager.Value?.MainCamera;

        // Return if the main camera is null
        if (mainCam == null)
            return;

        // Get the screen dimensions
        var screenDimensions = new Vector2(Screen.width, Screen.height);

        Enemy cEnemy = null;
        float cDistance = 0;
        var selectedCenter = Vector3.zero;


        // Get all the enemies in the scene
        foreach (var enemy in Enemy.Enemies)
        {
            // If the enemy is null, continue
            if (enemy == null)
                continue;

            var center = enemy.CenterTransform.position;

            // Get the enemy's position
            _enemyPosition = center;

            // Get the world to screen point of the enemy's position
            var screenPoint = mainCam.WorldToScreenPoint(_enemyPosition);

            // If the enemy is behind the camera, continue
            if (screenPoint.z < 0)
                continue;

            // Check if the screen point is within the actual aim square dimensions
            if (screenPoint.x < screenDimensions.x / 2f - ActualAimSquareSize / 2 ||
                screenPoint.x > screenDimensions.x / 2f + ActualAimSquareSize / 2 ||
                screenPoint.y < screenDimensions.y / 2f - ActualAimSquareSize / 2 ||
                screenPoint.y > screenDimensions.y / 2f + ActualAimSquareSize / 2
               )
                continue;

            // Get the distance between the enemy's screen position and the center of the screen
            var screenDistance = Vector2.Distance(screenPoint, screenDimensions / 2);

            // get the distance between the enemy and the player
            var distance = Vector3.Distance(
                ParentComponent.transform.position,
                _enemyPosition
            );

            // If the distance is too far away, continue
            if (distance > maxDistance)
                continue;

            // If the current enemy is closer than the previous enemy, set the current enemy to the current enemy
            if (cEnemy != null && screenDistance >= cDistance)
                continue;

            // Perform a raycast from the camera to the enemy, checking if the enemy is visible
            var hit = Physics.Raycast(
                ParentComponent.WeaponManager.FireTransform.position,
                _enemyPosition - ParentComponent.WeaponManager.FireTransform.position,
                out var hitInfo,
                distance,
                ~enemyLayerMask
            );

            // If the raycast hits anything, continue
            if (hit)
                continue;

            // Set the current enemy to the current enemy
            cEnemy = enemy;
            cDistance = screenDistance;
            selectedCenter = center;
            EnemyScreenPosition = screenPoint;
        }

        // Set the selected enemy to the current enemy
        SelectedEnemy = cEnemy != null ? cEnemy : Option<Enemy>.None;
        _enemyPosition = selectedCenter;
    }

    private void OnGUI()
    {
        if (!showDebug)
            return;

        // Draw a box at the center of the screen
        GUI.Box(
            new Rect(
                Screen.width / 2f - ActualAimSquareSize / 2,
                Screen.height / 2f - ActualAimSquareSize / 2,
                ActualAimSquareSize,
                ActualAimSquareSize
            ),
            ""
        );

        const int squareSize = 64;

        // Draw a red box at the selected enemy's position
        if (!SelectedEnemy.HasValue)
            return;

        // Return if the camera manager's value is null
        if (cameraManager?.Value == null)
            return;

        var screenPoint = cameraManager.Value.MainCamera.WorldToScreenPoint(_enemyPosition);

        GUI.Box(
            new Rect(
                screenPoint.x - squareSize / 2f,
                Screen.height - screenPoint.y - squareSize / 2f,
                squareSize,
                squareSize
            ),
            ""
        );
    }
}