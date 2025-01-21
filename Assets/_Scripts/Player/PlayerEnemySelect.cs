using System;
using UnityEngine;

public class PlayerEnemySelect : ComponentScript<Player>
{
    [SerializeField] private Vector2 originalScreenSize = new Vector2(1920, 1080);
    [SerializeField] private float aimSquareSize = 400;
    [SerializeField] private float maxDistance = 100;

    [SerializeField] private LayerMask enemyLayerMask;
    
    [SerializeField] private bool showDebug;

    private float ActualAimSquareSize => aimSquareSize * (Screen.width / originalScreenSize.x);

    public Enemy SelectedEnemy { get; private set; }

    private void FixedUpdate()
    {
        // Get the main camera
        var mainCam = Camera.main;

        // If the main camera is null, return
        if (mainCam == null)
            return;

        // Get the screen dimensions
        var screenDimensions = new Vector2(Screen.width, Screen.height);

        Enemy cEnemy = null;
        float cDistance = 0;

        // Get all the enemies in the scene
        foreach (var enemy in Enemy.Enemies)
        {
            // If the enemy is null, continue
            if (enemy == null)
                continue;

            // Get the enemy's position
            var enemyPosition = enemy.EnemyInfo.ParentComponent.transform.position;

            // Get the world to screen point of the enemy's position
            var screenPoint = mainCam.WorldToScreenPoint(enemyPosition);

            // If the enemy is behind the camera, continue
            if (screenPoint.z < 0)
                continue;

            // Check if the screen point is within the actual aim square dimensions
            if (screenPoint.x < Screen.width / 2f - ActualAimSquareSize / 2 ||
                screenPoint.x > Screen.width / 2f + ActualAimSquareSize / 2 ||
                screenPoint.y < Screen.height / 2f - ActualAimSquareSize / 2 ||
                screenPoint.y > Screen.height / 2f + ActualAimSquareSize / 2
               )
                continue;

            // Get the distance between the enemy's position and the player's position
            var distance = Vector3.Distance(ParentComponent.Rigidbody.transform.position, enemyPosition);

            // If the distance is too far away, continue
            if (distance > maxDistance)
                continue;

            // If the current enemy is closer than the previous enemy, set the current enemy to the current enemy
            if (cEnemy != null && distance >= cDistance)
                continue;

            // Perform a raycast from the camera to the enemy, checking if the enemy is visible
            var hit = Physics.Raycast(
                ParentComponent.WeaponManager.FireTransform.position,
                enemy.EnemyDetectionBehavior.DetectionOrigin.position -
                ParentComponent.WeaponManager.FireTransform.position,
                out var hitInfo,
                enemyLayerMask
            );

            // If the raycast hits nothing, continue
            if (!hit)
                continue;

            // If the hit object is not the enemy, continue
            if (!hitInfo.collider.TryGetComponentInParent(out Enemy hitEnemy) || hitEnemy != enemy)
                continue;

            // Set the current enemy to the current enemy
            cEnemy = enemy;
            cDistance = distance;
        }

        // Set the selected enemy to the current enemy
        SelectedEnemy = cEnemy;

        Debug.Log(SelectedEnemy);
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

        // Draw a red box at the selected enemy's position
        if (SelectedEnemy != null)
        {
            var enemyPosition = SelectedEnemy.EnemyInfo.ParentComponent.transform.position;
            var screenPoint = Camera.main.WorldToScreenPoint(enemyPosition);
            GUI.Box(
                new Rect(
                    screenPoint.x - 10,
                    Screen.height - screenPoint.y - 10,
                    20,
                    20
                ),
                ""
            );
        }
    }
}