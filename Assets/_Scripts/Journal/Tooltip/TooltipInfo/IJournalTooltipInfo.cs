using System;
using UnityEngine;
using UnityEngine.UI;

public interface IJournalTooltipInfo
{
    public Func<string> Text { get; }

    public JournalTooltipType TooltipType { get; }
}