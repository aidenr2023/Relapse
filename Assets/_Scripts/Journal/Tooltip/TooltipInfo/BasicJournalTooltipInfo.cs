using System;

public struct BasicJournalTooltipInfo : IJournalTooltipInfo
{
    public Func<string> Text { get; }
    public JournalTooltipType TooltipType { get; }

    public BasicJournalTooltipInfo(string text, JournalTooltipType tooltipType)
    {
        Text = () => text;
        TooltipType = tooltipType;
    }
}