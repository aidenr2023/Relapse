using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Playerinfo : MonoBehaviour
{
    public float maxHealth = 3f;
    public float health;
    public WinLose winLose; // Reference to the WinLose script

    void Start()
    {
        health = maxHealth;
    }

    {
        health -= damageAmount;
        health = Mathf.Clamp(health, 0, maxHealth);

        if (health <= 0)
        {
            // Trigger the lose condition
            if (winLose != null)
            {
                winLose.ShowLoseScreen();
                Debug.Log("Player died!");
            }
        }
    }
}