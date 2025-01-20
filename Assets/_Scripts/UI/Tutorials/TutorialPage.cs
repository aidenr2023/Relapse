using System;
using UnityEngine;
using UnityEngine.Video;

[Serializable]
public struct TutorialPage
{
    [SerializeField] private string subTitle;
    [SerializeField] private VideoClip videoClip;
    [SerializeField] private TutorialButton button;
    [SerializeField, TextArea] private string description;

    public string SubTitle => subTitle;
    public VideoClip VideoClip => videoClip;
    public string Description => description;


    public enum TutorialButton
    {
        None,
        Shoot,
        Reload,

        Interact,

        UsePower,
        ChangePower,

        Move,
        Sprint,
        Jump,
        Slide,

        Pause,
    }
}