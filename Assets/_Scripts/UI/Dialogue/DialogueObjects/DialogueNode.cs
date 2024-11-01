using UnityEngine;
using UnityEngine.Serialization;

public abstract class DialogueNode : ScriptableObject
{
    #region Serialized Fields

    [SerializeField] protected DialogueSpeakerInfo speakerInfo;

    #endregion

    #region Getters

    public DialogueSpeakerInfo SpeakerInfo => speakerInfo;

    public abstract string DialogueText { get; }

    #endregion

    public abstract DialogueNode GetNextNode();
}