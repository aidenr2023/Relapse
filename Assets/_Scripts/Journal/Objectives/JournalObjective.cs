using UnityEngine;

[CreateAssetMenu(fileName = "New Journal Objective", menuName = "Objective")]
public class JournalObjective : ScriptableObject
{
    [SerializeField] [TextArea(1, 2)] private string shortDescription;
    [SerializeField] [TextArea(1, 8)] private string longDescription;

    public string ShortDescription => shortDescription;

    public string LongDescription => longDescription;

    public string TooltipText => ShortDescription;
}