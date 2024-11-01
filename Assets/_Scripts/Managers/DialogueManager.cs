using System;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }


    [SerializeField] private DialogueUI dialogueUI;

    [SerializeField] [Range(0, 60)] private float charactersPerSecond = 8;

    public DialogueUI DialogueUI => dialogueUI;

    public float CharactersPerSecond => charactersPerSecond;

    private void Awake()
    {
        // Set the instance to this
        Instance = this;
    }
}