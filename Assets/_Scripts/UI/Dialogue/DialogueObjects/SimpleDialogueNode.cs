using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/Simple Node", order = 0)]
public sealed class SimpleDialogueNode : DialogueNode
{
    [SerializeField] protected DialogueNode nextDialogue;

    public override DialogueNode GetNextNode()
    {
        return nextDialogue;
    }
}