using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Speaker Info", menuName = "Dialogue/Speaker Info", order = 0)]
public sealed class DialogueSpeakerInfo : ScriptableObject
{
    #region Serialized Fields

    [SerializeField] private string speakerName;
    [SerializeField] private Sprite npcSprite;
    [SerializeField] private Color textColor;

    #endregion

    #region Getters

    public string SpeakerName => speakerName;
    public Sprite NpcSprite => npcSprite;
    public Color TextColor => textColor;

    #endregion
}