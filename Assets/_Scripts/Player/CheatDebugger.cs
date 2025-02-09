using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheatDebugger : MonoBehaviour
{
    float tempPlayerHealth;
    float tempPlayerMaxHealth;
    bool godMode = false;

    float playerSpeed;
    double speedMultiplier = 1;

    // Start is called before the first frame update
    void Start()
    {
        SaveHealthValues();
        SaveSpeedValues();

        Debug.Log("Player Health: " + tempPlayerHealth);

    }

    // Update is called once per frame
    void Update()
    {

        //God Mode Controls
        if (Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log("God Mode Toggled");
            if (godMode != true) {
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
    void SaveHealthValues()
    {
        tempPlayerHealth = Player.Instance.PlayerInfo.CurrentHealth;
        tempPlayerMaxHealth = Player.Instance.PlayerInfo.MaxHealth;
    }

    void StartGodMode()
    {

        godMode = true;

        //Set player health and max health to high value

        //Why is this all read only ugh

        //Player.Instance.PlayerInfo.CurrentHealth = 100000;
        //Player.Instance.PlayerInfo.MaxHealth = 100000;

        //PlayerInfo.player.ChangeHealth(100000);
        //Player.PlayerInfo.ChangeHealth(100000,Player,this,this);
    }
    void EndGodMode()
    {
        godMode = false;
        //Set player health and max health back to original values

        //Player.Instance.PlayerInfo.CurrentHealth = tempPlayerHealth;
        //Player.Instance.PlayerInfo.MaxHealth = tempPlayerMaxHealth;
    }
    
    void SaveSpeedValues()
    {
        //playerSpeed = Player.Instance.PlayerMovementV2.Speed;
    }
    void IncreaseSpeed()
    {
        speedMultiplier += 0.5;
        //Player.Instance.PlayerMovementV2.Speed = playerSpeed*speedMultiplier;
    }
    void DecreaseSpeed()
    {
        speedMultiplier -= 0.5;
        //Player.Instance.PlayerMovementV2.Speed = playerSpeed * speedMultiplier;
    }
}
