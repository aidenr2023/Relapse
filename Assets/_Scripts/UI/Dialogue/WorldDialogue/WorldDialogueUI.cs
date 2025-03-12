using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class WorldDialogueUI : MonoBehaviour
{
    private static event Action<WorldDialogue> OnStartDialogue;
    
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField, Range(0, 1)] private float maxOpacity = 1;
    [SerializeField] private float fadeTime = .5f;

    private Coroutine _dialogueCoroutine;
    
    private void OnEnable()
    {
        // Subscribe to the event
        OnStartDialogue += StartDialogueSingle;
        
        // Set the opacity to 0
        canvasGroup.alpha = 0;
    }

    private void OnDisable()
    {
        // Stop any running coroutines
        if (_dialogueCoroutine != null)
        {
            StopCoroutine(_dialogueCoroutine);
            _dialogueCoroutine = null;
        }
        
        // Unsubscribe from the event
        OnStartDialogue -= StartDialogueSingle;
    }

    private void StartDialogueSingle(WorldDialogue dialogue)
    {
        // Stop any running coroutines
        if (_dialogueCoroutine != null)
        {
            StopCoroutine(_dialogueCoroutine);
            _dialogueCoroutine = null;
        }

        _dialogueCoroutine = StartCoroutine(DialogueCoroutine(dialogue));
    }

    private IEnumerator DialogueCoroutine(WorldDialogue dialogue)
    {
        dialogueText.text = dialogue.DialogueText;

        var startTime = Time.time;
        var introFadeEndTime = startTime + fadeTime;
        var outroFadeStartTime = startTime + dialogue.Duration - fadeTime;
        var endTime = startTime + dialogue.Duration;

        // Fade the dialogue in
        while (Time.time < introFadeEndTime)
        {
            canvasGroup.alpha = Mathf.Lerp(0, maxOpacity, (Time.time - startTime) / fadeTime);
            yield return null;
        }
        
        // Set the alpha to the max value
        canvasGroup.alpha = maxOpacity;
        
        // Wait for the dialogue fade out time
        while (Time.time < outroFadeStartTime)
            yield return null;
        
        // Fade the dialogue out
        while (Time.time < endTime)
        {
            canvasGroup.alpha = Mathf.Lerp(maxOpacity, 0, (Time.time - outroFadeStartTime) / fadeTime);
            yield return null;
        }
        
        // Set the alpha to 0
        canvasGroup.alpha = 0;
    }

    public static void StartDialogue(WorldDialogue dialogue)
    {
        OnStartDialogue?.Invoke(dialogue);
    }
}