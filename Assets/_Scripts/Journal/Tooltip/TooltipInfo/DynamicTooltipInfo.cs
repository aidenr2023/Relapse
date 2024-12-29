using System;

public struct DynamicTooltipInfo : IJournalTooltipInfo
{
    public Func<string> Text { get; }
    public JournalTooltipType TooltipType { get; }

    public DynamicTooltipInfo(Func<string> text, JournalTooltipType tooltipType)
    {
        Text = text;
        TooltipType = tooltipType;
    }
}