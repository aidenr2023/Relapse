using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue Object", order = 0)]
public class DialogueObject : ScriptableObject
{
    [SerializeField] private string npcName;

    #region Serialized Fields

    [SerializeField] private DialogueEntry[] entries;

    #endregion

    #region Getters

    public string NpcName => npcName;

    public DialogueEntry[] Entries => entries;

    #endregion
}