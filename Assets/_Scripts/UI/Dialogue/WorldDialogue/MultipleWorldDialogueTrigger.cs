using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class MultipleWorldDialogueTrigger : MonoBehaviour
{
    [SerializeField] private bool activateOnce;
    [SerializeField] private WorldDialogueLine[] worldDialogues;

    [SerializeField] private UnityEvent onTriggerEnter;

    private int _timesActivated;

    private Coroutine _dialogueCoroutine;

    private void OnTriggerEnter(Collider other)
    {
        // Return if the other object is not the player
        if (!other.CompareTag("Player"))
            return;

        if (activateOnce && _timesActivated > 0)
            return;
        
        // If there is already another coroutine running, stop it
        if (_dialogueCoroutine != null)
        {
            StopCoroutine(_dialogueCoroutine);
            _dialogueCoroutine = null;
        }

        // Increment the times activated
        _timesActivated++;

        // Start the dialogue coroutine
        _dialogueCoroutine = StartCoroutine(DialogueCoroutine(worldDialogues));

        // On trigger enter event
        onTriggerEnter.Invoke();
    }

    private static IEnumerator DialogueCoroutine(WorldDialogueLine[] dialogues)
    {
        foreach (var line in dialogues)
        {
            // Play the line and wait for it to finish
            if (line.worldDialogue != null)
                yield return WorldDialogueUI.StartDialogue(line.worldDialogue);

            // Invoke the on dialogue finished event
            line.onDialogueFinished.Invoke();
        }
    }

    public void ForceStart()
    {
        if (_dialogueCoroutine != null)
        {
            StopCoroutine(_dialogueCoroutine);
            _dialogueCoroutine = null;
        }

        _dialogueCoroutine = StartCoroutine(DialogueCoroutine(worldDialogues));
    }

    [Serializable]
    private struct WorldDialogueLine
    {
        public WorldDialogue worldDialogue;
        public UnityEvent onDialogueFinished;
    }
}