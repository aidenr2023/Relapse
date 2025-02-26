using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrentationController : MonoBehaviour
{
    /// <summary>
    /// This script is used to make the enemy face the player
    /// </summary>

    [SerializeField] private Transform player;
    [SerializeField] private float rotationSpeed = 180f; // Degrees per second

    //check for the player object instance at runtime
    private void Start()
    {
        if (player == null && Player.Instance != null)
        {
            player = Player.Instance.transform;
        }
    }
    
    //constantly update the rotation of the enemy to face the player
    void Update()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0; // Keep the rotation horizontal

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            ); 
        } 
    }
}
