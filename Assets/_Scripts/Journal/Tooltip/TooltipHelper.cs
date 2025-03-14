using UnityEngine;

public class TooltipHelper : MonoBehaviour
{
    [SerializeField] private string tooltipText;

    public void ShowTooltip()
    {
        // Create a new tooltip and add it to the TooltipManager
        JournalTooltipManager.Instance.AddTooltip(new BasicJournalTooltipInfo(tooltipText, JournalTooltipType.General));
    }
}