using System;
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
        // // Print out each line of dialogue
        // foreach (var entry in dialogueObject.Entries)
        // {
        //     var speaker = entry.Speaker switch
        //     {
        //         DialogueSpeaker.NPC => dialogueObject.NpcName,
        //         DialogueSpeaker.Player => "Player",
        //         DialogueSpeaker.Narrator => "",
        //         _ => ""
        //     };
        //
        //     Debug.Log($"{speaker}: {entry.Text}");
        // }

        DialogueManager.Instance.DialogueUI.StartDialogue(dialogue);
    }
}