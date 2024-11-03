using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/Dialogue Choice Node", order = 0)]
public sealed class DialogueChoiceNode : DialogueNode
{
    [SerializeField] private DialogueChoice[] dialogueChoices;

    public override string DialogueText => string.Empty;

    public IReadOnlyCollection<DialogueChoice> DialogueChoices => dialogueChoices;

    public override DialogueNode GetNextNode(int index = -1)
    {
        if (index >= 0 && index < dialogueChoices.Length)
            return dialogueChoices[index].NextDialogue;

        Debug.LogError("Invalid index for dialogue choice.");

        return null;
    }

    [Serializable]
    public class DialogueChoice
    {
        [SerializeField] private string displayText;
        [SerializeField] private DialogueNode nextDialogue;

        public string DisplayText => displayText;
        public DialogueNode NextDialogue => nextDialogue;
    }
}