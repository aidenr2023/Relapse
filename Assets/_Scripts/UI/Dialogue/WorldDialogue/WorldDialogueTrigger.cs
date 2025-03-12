using System;
using UnityEngine;
using UnityEngine.Events;

public class WorldDialogueTrigger : MonoBehaviour
{
    [SerializeField] private WorldDialogue worldDialogue;
    [SerializeField] private bool activateOnce;

    [SerializeField] private UnityEvent onTriggerEnter;
    
    private int _timesActivated;

    private void OnTriggerEnter(Collider other)
    {
        // Return if the other object is not the player
        if (!other.CompareTag("Player"))
            return;

        if (activateOnce && _timesActivated > 0)
            return;

        // Increment the times activated
        _timesActivated++;

        // Activate the dialogue
        WorldDialogueUI.StartDialogue(worldDialogue);
        
        // On trigger enter event
        onTriggerEnter.Invoke();
    }
}