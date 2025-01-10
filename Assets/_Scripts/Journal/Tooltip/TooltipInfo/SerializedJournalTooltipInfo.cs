using System;
using UnityEngine;

[Serializable]
public struct SerializedJournalTooltipInfo : IJournalTooltipInfo
{
    #region Serialized Fields

    [SerializeField] private string text;
    [SerializeField] private JournalTooltipType tooltipType;

    #endregion

    #region Getters

    public Func<string> Text
    {
        get
        {
            var tmpText = text;
            return () => tmpText;
        }
    }

    public JournalTooltipType TooltipType => tooltipType;

    #endregion
}