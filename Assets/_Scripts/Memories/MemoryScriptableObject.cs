using UnityEngine;

[CreateAssetMenu(fileName = "Memory", menuName = "Memory")]
public class MemoryScriptableObject : ScriptableObject
{
    #region Serialized Fields

    [SerializeField] private string memoryName;
    [SerializeField] [TextArea(1, 8)] private string shortDescription;
    [SerializeField] [TextArea(1, 16)] private string longDescription;
    [SerializeField] private Sprite memoryImage;

    #endregion

    #region Getters

    public string MemoryName => memoryName;

    public string ShortDescription => shortDescription;

    public string LongDescription => longDescription;

    public Sprite MemoryImage => memoryImage;

    #endregion
}