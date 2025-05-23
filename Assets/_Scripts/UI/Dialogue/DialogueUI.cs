﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private UnityEvent OnDialogueEnd;

    [Header("Text")] [SerializeField] private GameObject textBoxParent;
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text dialogueText;

    [Header("Image")] [SerializeField] private GameObject npcImageParent;
    [SerializeField] private Image npcImage;

    [Header("Buttons")] [SerializeField] private GameObject dialogueButtonsParent;
    [SerializeField] private Button[] buttons = new Button[4];

    [SerializeField] private Button nextButton;

    #endregion

    #region Getters

    public DialogueNode CurrentDialogue => _currentDialogue;

    public TMP_Text DialogueText => dialogueText;
    
    public Button NextButton => nextButton;

    private bool IsTyping => _currentCharacterIndex < _currentDialogue.DialogueText.Length;
    
    public IReadOnlyList<Button> Buttons => buttons;

    #endregion

    #region Private Fields

    private DialogueNode _currentDialogue;

    private int _currentCharacterIndex;

    private readonly StringBuilder _currentText = new();

    private CountdownTimer _typingTimer;

    private List<DialogueNode> _dialogueNodes;

    private CountdownTimer _spriteAnimationTimer;
    private int _currentSpriteIndex;

    #endregion

    private void Awake()
    {
        _dialogueNodes = new();

        _typingTimer = new CountdownTimer(1f, true, false);

        _typingTimer.OnTimerEnd += AddCharacterOnTimerEnd;

        // Set up the sprite animation timer
        _spriteAnimationTimer = new CountdownTimer(float.MaxValue, true);
        _spriteAnimationTimer.OnTimerEnd += AdvanceSpriteIndex;
        _spriteAnimationTimer.Start();
    }

    private void AdvanceSpriteIndex()
    {
        // Reset the timer
        _spriteAnimationTimer.Reset();
        
        // Return if the current dialogue is null
        if (_currentDialogue == null)
            return;

        // Return if the current dialogue's speaker info is null
        if (_currentDialogue.SpeakerInfo == null)
            return;

        // Return if the current dialogue's speaker info's NPC sprite is null
        if (_currentDialogue.SpeakerInfo.NpcSprites == null || _currentDialogue.SpeakerInfo.NpcSprites.Length == 0)
            return;

        // Increment the current sprite index
        _currentSpriteIndex = (_currentSpriteIndex + 1) % _currentDialogue.SpeakerInfo.NpcSprites.Length;

        // Set the NPC image
        npcImage.sprite = _currentDialogue.SpeakerInfo.NpcSprites[_currentSpriteIndex];
    }


    private void Start()
    {
        // Disable the dialogue buttons parent by default
        dialogueButtonsParent.SetActive(false);

        // // Set the dialogue UI to be invisible
        // SetVisibility(false);

        // Reset the current text
        ResetCurrentText();
    }

    private void Update()
    {
        // Update the typing timer
        UpdateTypingTimer();

        // Update the NPC image
        UpdateNpcImageTimer();

        // Update the dialogue
        UpdateDialogue();
    }

    private void UpdateNpcImageTimer()
    {
        // Set the sprite timer to active
        _spriteAnimationTimer.SetActive(true);
        
        if (_currentDialogue != null)
            _spriteAnimationTimer.SetMaxTime(1f / _currentDialogue.SpeakerInfo.FramesPerSecond);

        // Update the sprite animation timer
        _spriteAnimationTimer.Update(Time.unscaledDeltaTime);

        SetNpcImage();
    }

    private void UpdateTypingTimer()
    {
        // Return if there is an active menu other than the vendor menu that pauses the game
        var activeMenus = MenuManager.Instance.ActiveMenus;
        
        if (activeMenus.Any(menu => menu.PausesGame && menu != VendorMenu.Instance))
            return;
        
        // Return if the current dialogue is null
        if (_currentDialogue == null)
            return;

        // If we are no longer typing, return
        if (!IsTyping)
            return;

        // Set the timer's max time based on the characters per second
        _typingTimer.SetMaxTime(1f / DialogueManager.Instance.CharactersPerSecond);

        _typingTimer.Update(Time.unscaledDeltaTime);
    }

    private void UpdateDialogue()
    {
        // Return if the current dialogue is null
        if (_currentDialogue == null)
            return;

        // If we are no longer typing, return
        if (!IsTyping)
            return;

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
        // // Debug.Log($"Starting Dialogue with {dialogueNode.NpcName}");
        // Debug.Log($"Starting Dialogue with {dialogueNode.SpeakerInfo.SpeakerName}");

        // Reset the current character index
        _currentCharacterIndex = 0;

        // Reset the current text
        ResetCurrentText();

        // Reset the typing timer
        _typingTimer.Reset();

        // Clear the dialogue nodes list
        _dialogueNodes.Clear();

        // Reset the visibilities
        ResetVisibilities();

        // Force the text to be visible
        textBoxParent.SetActive(true);

        // Set the current dialogue object
        SetCurrentDialogue(dialogueNode);

        // Reset the sprite index
        _currentSpriteIndex = 0;

        // Determine the npc image
        SetNpcImage();

        // Set the dialogue UI to be visible
        SetVisibility(true);

        // Debug.Log($"Setting the visibility to true for {gameObject.name}");
    }

    public void NextDialogue()
    {
        // Debug.Log($"Dialogue Manager instance: {DialogueManager.Instance}");
        // Debug.Log($"DialogueUI instance: {DialogueManager.Instance.DialogueUI}");
        // Debug.Log($"Current Dialogue: {DialogueManager.Instance.DialogueUI.CurrentDialogue}");

        if (_currentDialogue is DialogueChoiceNode)
            return;

        AdvanceDialogue();
    }

    private void AdvanceDialogue()
    {
        // Return if the current dialogue is null
        if (_currentDialogue == null)
            return;

        // Deactivate the dialogue buttons parent
        dialogueButtonsParent.SetActive(false);

        // Activate the text box parent
        textBoxParent.SetActive(true);

        // If we are still typing, finish typing
        if (IsTyping)
        {
            // Clear the current text without updating the UI
            _currentText.Clear();

            // Add all the text
            AddCurrentText(_currentDialogue.DialogueText);

            // Set the character index
            _currentCharacterIndex = _currentDialogue.DialogueText.Length;
        }
        else
        {
            // If we are not typing, reset the current text
            ResetCurrentText();

            // Reset the current character index
            _currentCharacterIndex = 0;

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
                // End the current dialogue
                _currentDialogue.OnDialogueEnd.Invoke();

                // Add the current dialogue to the list
                SetCurrentDialogue(nextNode);

                // Invoke the next dialogue's start event
                _currentDialogue.OnDialogueStart.Invoke();

                // Debug.Log($"Now using: {_currentDialogue} - {_currentDialogue.DialogueText}");
            }
        }

        // Set the NPC image
        SetNpcImage();

        // Reset the typing timer
        _typingTimer.Reset();

        // If the current dialogue is null, invoke the OnDialogueEnd event
        if (_currentDialogue == null)
            OnDialogueEnd?.Invoke();
    }

    private void SetNpcImage()
    {
        // Disable the NPC image by default
        npcImageParent.gameObject.SetActive(false);

        // Return if the current dialogue is null
        if (_currentDialogue == null)
            return;

        // Return if the current dialogue's speaker info is null
        if (_currentDialogue.SpeakerInfo == null)
            return;

        // Return if the current dialogue's speaker info's NPC sprite is null
        if (_currentDialogue.SpeakerInfo.NpcSprites == null || _currentDialogue.SpeakerInfo.NpcSprites.Length == 0)
            return;

        // Activate the NPC image
        npcImageParent.gameObject.SetActive(true);

        // Set the NPC image
        npcImage.sprite = _currentDialogue.SpeakerInfo.NpcSprites[_currentSpriteIndex];
    }

    private void SetCurrentDialogue(DialogueNode dialogueNode)
    {
        // set the current dialogue object
        _currentDialogue = dialogueNode;

        // Add the node to the list of dialogues
        _dialogueNodes.Add(dialogueNode);

        // Extra logic for choice nodes
        // TODO: Put this in a better place or something. This goes against SOLID
        if (_currentDialogue is DialogueChoiceNode dialogueChoiceNode)
            ActivateChoiceButtons(dialogueChoiceNode);
        else
        {
            // Select the next button
            nextButton.Select();
            
            VendorMenu.Instance.EventSystem.SetSelectedGameObject(nextButton.gameObject);
        }

        if (nextButton != null)
            nextButton.gameObject.SetActive(_currentDialogue is not DialogueChoiceNode);
    }

    private void ActivateChoiceButtons(DialogueChoiceNode dialogueChoiceNode)
    {
        // Stop typing
        _typingTimer.Stop();

        // Activate the dialogue buttons parent
        dialogueButtonsParent.SetActive(true);

        // Deactivate the text box parent
        textBoxParent.SetActive(false);

        // Deactivate all buttons
        foreach (var button in buttons)
        {
            button.gameObject.SetActive(false);

            // Remove all listeners from the button
            button.onClick.RemoveAllListeners();
        }

        for (var i = 0; i < dialogueChoiceNode.DialogueChoices.Count; i++)
        {
            var cButton = buttons[i];

            var displayText = cButton.GetComponentInChildren<TMP_Text>();
            displayText.text = dialogueChoiceNode.DialogueChoices.ElementAt(i).DisplayText;

            // Activate the button
            cButton.gameObject.SetActive(true);

            var choice = dialogueChoiceNode.DialogueChoices.ElementAt(i);

            cButton.onClick.AddListener(() =>
            {
                // Set the current dialogue to the next dialogue
                SetCurrentDialogue(choice.NextDialogue);

                // Call the next dialogue
                AdvanceDialogue();

                // Reset the current text
                ResetCurrentText();
                _currentCharacterIndex = 0;

                // Resume typing
                _typingTimer.Reset();
                _typingTimer.Start();

                Debug.Log($"ADVANCED DIALOGUE TO: {choice.NextDialogue}");
            });
        }

        // Set up navigation
        for (var i = 0; i < buttons.Length; i++)
        {
            var button = buttons[i];

            var nav = button.navigation;
            nav.mode = Navigation.Mode.Explicit;

            if (i == 0)
            {
                nav.selectOnDown = buttons[i + 1];
                nav.selectOnUp = buttons[^1];
            }
            else if (i == buttons.Length - 1)
            {
                nav.selectOnUp = buttons[i - 1];
                nav.selectOnDown = buttons[0];
            }
            else
            {
                nav.selectOnDown = buttons[i + 1];
                nav.selectOnUp = buttons[i - 1];
            }

            button.navigation = nav;
        }

        // Set the event system's selected object to the first button
        buttons[0].Select();
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

    private void ResetVisibilities()
    {
        // Deactivate the dialogue buttons parent
        dialogueButtonsParent.SetActive(false);

        // Activate the text box parent
        textBoxParent.SetActive(true);

        // Deactivate the NPC image parent
        npcImageParent.SetActive(false);
    }
}