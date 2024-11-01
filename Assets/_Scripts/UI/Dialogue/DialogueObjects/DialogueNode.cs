using UnityEngine;
using UnityEngine.Serialization;

// [CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/Dialogue Object", order = 0)]
public abstract class DialogueNode : ScriptableObject
{
    #region Serialized Fields

    [SerializeField] protected DialogueSpeakerInfo speakerInfo;

    [SerializeField] [TextArea] protected string dialogueText;

    #endregion

    #region Getters

    public DialogueSpeakerInfo SpeakerInfo => speakerInfo;

    public string DialogueText => dialogueText;

    #endregion

    public abstract DialogueNode GetNextNode();
}