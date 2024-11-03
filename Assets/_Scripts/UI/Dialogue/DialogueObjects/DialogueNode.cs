using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public abstract class DialogueNode : ScriptableObject
{
    #region Serialized Fields

    [Header("Events")] [SerializeField] public UnityEvent OnDialogueStart;
    [SerializeField] public UnityEvent OnDialogueEnd;

    [Space] [SerializeField] protected DialogueSpeakerInfo speakerInfo;

    #endregion

    #region Getters

    public DialogueSpeakerInfo SpeakerInfo => speakerInfo;

    public abstract string DialogueText { get; }

    #endregion

    public abstract DialogueNode GetNextNode(int index = 0);
}