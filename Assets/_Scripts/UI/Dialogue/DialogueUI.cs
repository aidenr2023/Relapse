using System;
using System.Text;
using TMPro;
using UnityEngine;

public class DialogueUI : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private TMP_Text dialogueText;

    #endregion

    #region Getters

    public TMP_Text DialogueText => dialogueText;

    private bool IsTyping => _currentCharacterIndex < _currentDialogue.Entries[_currentDialogueIndex].Text.Length;

    #endregion

    #region Private Fields

    private DialogueObject _currentDialogue;

    /// <summary>
    /// Which dialogue entry we are currently on.
    /// </summary>
    private int _currentDialogueIndex;

    private int _currentCharacterIndex;

    private StringBuilder _currentText = new();

    private CountdownTimer _typingTimer = new CountdownTimer(1f, true, false);

    #endregion

    private void Awake()
    {
        _typingTimer.OnTimerEnd += AddCharacterOnTimerEnd;
    }


    private void Start()
    {
        // Set the dialogue UI to be invisible
        SetVisibility(false);

        _currentDialogueIndex = 0;

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

    public void StartDialogue(DialogueObject dialogueObject)
    {
        Debug.Log($"Starting Dialogue with {dialogueObject.NpcName}");

        // Set the current dialogue object
        _currentDialogue = dialogueObject;

        // Set the current dialogue index to 0
        _currentDialogueIndex = 0;

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
        // If we are still typing, finish typing
        if (IsTyping)
        {
            // Clear the current text without updating the UI
            _currentText.Clear();

            // Add all the text
            AddCurrentText(_currentDialogue.Entries[_currentDialogueIndex].Text);

            // Set the character index
            _currentCharacterIndex = _currentDialogue.Entries[_currentDialogueIndex].Text.Length;
            return;
        }
        else
        {
            // If we are not typing, reset the current text
            ResetCurrentText();

            // Increment the current dialogue index
            _currentDialogueIndex++;
        }
    }

    private void AddCharacterOnTimerEnd()
    {
        // Get the current text from the dialogue
        var currentText = _currentDialogue.Entries[_currentDialogueIndex].Text;

        // Add the current character to the text
        AddCurrentText(currentText[_currentCharacterIndex]);
        _currentCharacterIndex++;

        // Reset the timer
        _typingTimer.Reset();
    }
}