using System;
using UnityEngine;

[Serializable]
public class DialogueEntry
{
    #region Serialized Fields

    [SerializeField] private DialogueSpeaker speaker;
    [SerializeField] [TextArea] private string text;

    #endregion

    #region Getters

    public DialogueSpeaker Speaker => speaker;

    public string Text => text;

    #endregion
}