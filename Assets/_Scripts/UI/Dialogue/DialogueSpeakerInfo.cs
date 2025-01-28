using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Speaker Info", menuName = "Dialogue/Speaker Info", order = 0)]
public sealed class DialogueSpeakerInfo : ScriptableObject
{
    #region Serialized Fields

    [SerializeField] private string speakerName;
    [SerializeField] private Sprite[] npcSprites;
    [SerializeField, Min(0)] private int framesPerSecond = 12;
    [SerializeField] private Color textColor;

    #endregion

    #region Getters

    public string SpeakerName => speakerName;
    public Sprite[] NpcSprites => npcSprites;
    public int FramesPerSecond => framesPerSecond;
    public Color TextColor => textColor;

    #endregion
}