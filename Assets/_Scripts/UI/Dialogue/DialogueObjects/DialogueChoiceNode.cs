using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/Dialogue Choice Node", order = 0)]
public sealed class DialogueChoiceNode : DialogueNode
{
    [SerializeField] private DialogueChoice[] dialogueChoices;

    public override string DialogueText => string.Empty;

    public override DialogueNode GetNextNode()
    {
        return dialogueChoices[0].NextDialogue;
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