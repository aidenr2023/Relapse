using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/Simple Node", order = 0)]
public sealed class SimpleDialogueNode : DialogueNode
{
    [SerializeField] private DialogueNode nextDialogue;
    [SerializeField] [TextArea] private string dialogueText;

    public override string DialogueText => dialogueText;

    public override DialogueNode GetNextNode(int index = 0)
    {
        return nextDialogue;
    }
}