using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Tutorial", menuName = "New Tutorial")]
public class Tutorial : ScriptableObject
{
    private static readonly HashSet<Tutorial> tutorials = new();

    public static IReadOnlyCollection<Tutorial> Tutorials => tutorials;

    [SerializeField] private string tutorialName;
    [SerializeField, UniqueIdentifier] private string uniqueId;
    [SerializeField] private TutorialPage[] tutorialPages;

    public string TutorialName => tutorialName;
    public IReadOnlyList<TutorialPage> TutorialPages => tutorialPages;
    public int TotalPages => tutorialPages.Length;

    public string UniqueId => uniqueId;

    public Tutorial()
    {
        // Add the tutorial to the hash set
        tutorials.Add(this);
    }
}