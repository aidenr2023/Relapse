using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.SceneManagement;


public class Checkpoint : MonoBehaviour
{
    public static Checkpoint Instance { get; private set; }

    [SerializeField] CheckpointInteractable[] checkpointList;

    int currentCheckpoint;
    int highestCheckpoint =-1;

    private PlayerInfo player;



    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<PlayerInfo>();
        player.OnDeath += LoadCheckpoint;
    }

    private void LoadCheckpoint(object sender, HealthChangedEventArgs e)
    {
        if (e.DamagerObject == e.Actor || highestCheckpoint == -1)
        {
            //Restart the scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            //Teleport player to highest checkpoint's respawnPosition
            player.transform.position = checkpointList[highestCheckpoint].RespawnPosition.position;

            //Reset player
            //player health is restored
            //Reset powers
            //Any other attributes that need to be reset are reset

            // Get the player component
            Player.Instance.ResetPlayer();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    //When player interacts with a burner phone, save the current checkpoint as the transform of the burner phone
    public void SaveCheckpoint(CheckpointInteractable interactedObject)
    {
        //Set current checkpoint to that object
        currentCheckpoint = Array.IndexOf(checkpointList,interactedObject);
        Debug.Log("Current: " + currentCheckpoint);

        //See if the checkpoint is higher than the highest checkpoint
        if (currentCheckpoint > highestCheckpoint)
        {
            highestCheckpoint = currentCheckpoint;
            Debug.Log("New Highest: " + currentCheckpoint);

        }
    }
}