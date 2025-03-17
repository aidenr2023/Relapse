using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Tutorial", menuName = "New Tutorial")]
public class Tutorial : ScriptableObject
{
    [SerializeField] private string tutorialName;
    [SerializeField] private TutorialType tutorialType;
    [SerializeField, UniqueIdentifier] private string uniqueId;
    [SerializeField] private TutorialPage[] tutorialPages;

    public string TutorialName => tutorialName;
    public TutorialType TutorialType => tutorialType;
    public IReadOnlyList<TutorialPage> TutorialPages => tutorialPages;
    public int TotalPages => tutorialPages.Length;

    public string UniqueId => uniqueId;

}