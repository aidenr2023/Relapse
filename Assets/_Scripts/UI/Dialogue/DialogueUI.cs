using System;
using System.Text;
using TMPro;
using UnityEngine;

public class DialogueUI : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text dialogueText;

    #endregion

    #region Getters

    public TMP_Text DialogueText => dialogueText;

    private bool IsTyping => _currentCharacterIndex < _currentDialogue.DialogueText.Length;

    #endregion

    #region Private Fields

    private DialogueNode _currentDialogue;

    private int _currentCharacterIndex;

    private readonly StringBuilder _currentText = new();

    private readonly CountdownTimer _typingTimer = new CountdownTimer(1f, true, false);

    #endregion

    private void Awake()
    {
        _typingTimer.OnTimerEnd += AddCharacterOnTimerEnd;
    }


    private void Start()
    {
        // Set the dialogue UI to be invisible
        SetVisibility(false);

        // Reset the current text
        ResetCurrentText();
    }

    private void Update()
    {
        // Update the typing timer
        UpdateTypingTimer();

        // Update the dialogue
        UpdateDialogue();
    }

    private void UpdateTypingTimer()
    {
        // Return if the current dialogue is null
        if (_currentDialogue == null)
            return;

        // If we are no longer typing, return
        if (!IsTyping)
            return;

        // Set the timer's max time based on the characters per second
        _typingTimer.SetMaxTime(1f / DialogueManager.Instance.CharactersPerSecond);

        _typingTimer.Update(Time.deltaTime);
    }

    private void UpdateDialogue()
    {
        // Return if the current dialogue is null
        if (_currentDialogue == null)
            return;

        // If we are no longer typing, return
        if (!IsTyping)
            return;

        // // Set the speaker Text
        // speakerText.text = _currentDialogue.Entries[_currentDialogueIndex].Speaker switch
        // {
        //     DialogueSpeaker.NPC => _currentDialogue.NpcName,
        //     DialogueSpeaker.Player => "Player",
        //     DialogueSpeaker.Narrator => "Narrator",
        //     _ => "UNHANDLED SPEAKER!!!"
        // };

        // Set the speaker Text
        speakerText.text = _currentDialogue.SpeakerInfo.SpeakerName;
    }

    private void ResetCurrentText()
    {
        // Reset the current text
        _currentText.Clear();

        // Update the text UI
        UpdateTextUI();
    }

    private void AddCurrentText(string text)
    {
        // Update the current text
        _currentText.Append(text);

        // Update the text UI
        UpdateTextUI();
    }

    private void AddCurrentText(char text)
    {
        // Update the current text
        _currentText.Append(text);

        // Update the text UI
        UpdateTextUI();
    }

    private void UpdateTextUI()
    {
        // Update the text on the UI
        dialogueText.text = _currentText.ToString();
    }

    private void SetVisibility(bool visible)
    {
        gameObject.SetActive(visible);
    }

    public void StartDialogue(DialogueNode dialogueNode)
    {
        // Debug.Log($"Starting Dialogue with {dialogueNode.NpcName}");
        Debug.Log($"Starting Dialogue with {dialogueNode.SpeakerInfo.SpeakerName}");

        // Set the current dialogue object
        _currentDialogue = dialogueNode;

        // Reset the current character index
        _currentCharacterIndex = 0;

        // Reset the current text
        ResetCurrentText();

        // Reset the typing timer
        _typingTimer.Reset();

        // Set the dialogue UI to be visible
        SetVisibility(true);
    }

    public void NextDialogue()
    {
        // Return if the current dialogue is null
        if (_currentDialogue == null)
            return;

        // If we are still typing, finish typing
        if (IsTyping)
        {
            // Clear the current text without updating the UI
            _currentText.Clear();

            // Add all the text
            // AddCurrentText(_currentDialogue.Entries[_currentDialogueIndex].Text);
            AddCurrentText(_currentDialogue.DialogueText);

            // Set the character index
            // _currentCharacterIndex = _currentDialogue.Entries[_currentDialogueIndex].Text.Length;
            _currentCharacterIndex = _currentDialogue.DialogueText.Length;
        }
        else
        {
            // If we are not typing, reset the current text
            ResetCurrentText();

            // Reset the current character index
            _currentCharacterIndex = 0;

            // // If the current dialogue index is greater than or equal to the number of entries, end the dialogue
            // if (_currentDialogueIndex >= _currentDialogue.Entries.Length)
            // {
            //     // Set the dialogue UI to be invisible
            //     SetVisibility(false);
            //
            //     // Set the current dialogue to null
            //     _currentDialogue = null;
            // }

            var nextNode = _currentDialogue.GetNextNode();

            // If there are no more dialogue nodes, end the dialogue
            if (nextNode == null)
            {
                // Set the dialogue UI to be invisible
                SetVisibility(false);

                // Set the current dialogue to null
                _currentDialogue = null;
            }
            else
            {
                _currentDialogue = nextNode;

                Debug.Log($"Now using: {_currentDialogue} - {_currentDialogue.DialogueText}");
            }
        }

        // Reset the typing timer
        _typingTimer.Reset();
    }

    private void AddCharacterOnTimerEnd()
    {
        // Get the current text from the dialogue
        // var currentText = _currentDialogue.Entries[_currentDialogueIndex].Text;
        var currentText = _currentDialogue.DialogueText;

        // Add the current character to the text
        AddCurrentText(currentText[_currentCharacterIndex]);
        _currentCharacterIndex++;

        // Reset the timer
        _typingTimer.Reset();
    }
}