using System;
using UnityEngine;
using UnityEngine.Video;

[Serializable]
public struct TutorialPage
{
    [SerializeField] private string subTitle;
    [SerializeField] private VideoClip videoClip;
    [SerializeField, TextArea] private string description;

    public string SubTitle => subTitle;
    public VideoClip VideoClip => videoClip;
    public string Description => description;
}