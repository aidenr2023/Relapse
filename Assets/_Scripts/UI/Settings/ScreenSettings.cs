using System;
using UnityEngine;

[Serializable]
public struct ScreenSettings
{
    [field: SerializeField] public FullScreenMode FullScreenMode { get; set; }
    [field: SerializeField] public ResolutionSizeStruct Resolution { get; set; }
    [field: SerializeField] public int QualityLevel { get; set; }
    [field: SerializeField] public bool IsVSync { get; set; }
    [field: SerializeField] public int AntiAliasing { get; set; }
    [field: SerializeField] public RefreshRateStruct RefreshRate { get; set; }

    [Serializable]
    public struct ResolutionSizeStruct
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }

    [Serializable]
    public struct RefreshRateStruct
    {
        public uint Numerator { get; set; }
        public uint Denominator { get; set; }
    }
}