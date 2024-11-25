using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class DialogueInteractable : MonoBehaviour, IInteractable
{
    #region Serialized Fields

    [FormerlySerializedAs("dialogueObject")] [SerializeField] private DialogueNode dialogueNode;

    #endregion

    #region Getters

    public GameObject GameObject => gameObject;

    public bool IsInteractable => true;

    public bool HasOutline { get; set; }

    public HashSet<Material> OutlineMaterials { get; } = new();

    #endregion

    public void Interact(PlayerInteraction playerInteraction)
    {
        StartDialogue(dialogueNode);
    }

    public void LookAtUpdate(PlayerInteraction playerInteraction)
    {
    }

    public string InteractText(PlayerInteraction playerInteraction)
    {
        return $"Talk to {dialogueNode.SpeakerInfo.SpeakerName}";
    }

    public void StartDialogue(DialogueNode dialogue)
    {
        DialogueManager.Instance.DialogueUI.StartDialogue(dialogue);
    }
}