using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheatDebugger : MonoBehaviour
{
    private float tempPlayerHealth;
    private float tempPlayerMaxHealth;
    private bool godMode = false;

    private float playerSpeed;
    private double speedMultiplier = 1;

    // Start is called before the first frame update
    private void Start()
    {
        SaveHealthValues();
        SaveSpeedValues();

        Debug.Log("Player Health: " + tempPlayerHealth);
    }

    // Update is called once per frame
    private void Update()
    {
        return;

        //God Mode Controls
        if (Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log("God Mode Toggled");
            if (godMode != true)
            {
                StartGodMode();
            }
            else
            {
                EndGodMode();
            }
        }


        //Speed Controls
        if (Input.GetKeyDown(KeyCode.H))
        {
            Debug.Log("Speed Increased");
            IncreaseSpeed();
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            Debug.Log("Speed Decreased");
            DecreaseSpeed();
        }
    }

    private void SaveHealthValues()
    {
        tempPlayerHealth = Player.Instance.PlayerInfo.CurrentHealth;
        tempPlayerMaxHealth = Player.Instance.PlayerInfo.MaxHealth;
    }

    private void StartGodMode()
    {
        godMode = true;

        //Set player health and max health to high value

        //Why is this all read only ugh

        //Player.Instance.PlayerInfo.CurrentHealth = 100000;
        //Player.Instance.PlayerInfo.MaxHealth = 100000;

        //PlayerInfo.player.ChangeHealth(100000);
        //Player.PlayerInfo.ChangeHealth(100000,Player,this,this);
    }

    private void EndGodMode()
    {
        godMode = false;
        //Set player health and max health back to original values

        //Player.Instance.PlayerInfo.CurrentHealth = tempPlayerHealth;
        //Player.Instance.PlayerInfo.MaxHealth = tempPlayerMaxHealth;
    }

    private void SaveSpeedValues()
    {
        //playerSpeed = Player.Instance.PlayerMovementV2.Speed;
    }

    private void IncreaseSpeed()
    {
        speedMultiplier += 0.5;
        //Player.Instance.PlayerMovementV2.Speed = playerSpeed*speedMultiplier;
    }

    private void DecreaseSpeed()
    {
        speedMultiplier -= 0.5;
        //Player.Instance.PlayerMovementV2.Speed = playerSpeed * speedMultiplier;
    }
}