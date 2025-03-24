using System;
using UnityEngine;
using UnityEngine.Events;

public class RandomWorldDialogueTrigger : MonoBehaviour
{
    [SerializeField] private WorldDialogue[] worldDialogues;
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

        // Get a random dialogue from the list of world dialogues
        var randomDialogue = worldDialogues[UnityEngine.Random.Range(0, worldDialogues.Length)];

        // Activate the dialogue
        WorldDialogueUI.StartDialogue(randomDialogue);

        // On trigger enter event
        onTriggerEnter.Invoke();
    }

    public void ForceStart()
    {
        // Get a random dialogue from the list of world dialogues
        var randomDialogue = worldDialogues[UnityEngine.Random.Range(0, worldDialogues.Length)];

        // Activate the dialogue
        WorldDialogueUI.StartDialogue(randomDialogue);
    }
}