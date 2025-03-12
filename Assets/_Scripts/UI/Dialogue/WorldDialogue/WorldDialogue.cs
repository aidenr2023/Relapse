using UnityEngine;

[CreateAssetMenu(fileName = "WorldDialogue", menuName = "Dialogue/World Dialogue")]
public class WorldDialogue : ScriptableObject
{
    [SerializeField, TextArea] private string dialogueText;

    [SerializeField, Min(0)] private float duration = 5;
    
    public string DialogueText => dialogueText;
    public float Duration => duration;
}