using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class JournalUIMemoryItem : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private MemoryScriptableObject memory;

    [SerializeField] private TMP_Text powerNameText;
    [SerializeField] private Image powerImage;

    [SerializeField] private Button button;

    [SerializeField] private EventTrigger eventTrigger;

    #endregion

    #region Getters

    public MemoryScriptableObject Memory => memory;

    public Button Button => button;

    public EventTrigger EventTrigger => eventTrigger;

    #endregion

    private void Update()
    {
        if (memory == null)
            throw new Exception("Power not set");

        // Update the power item data
        UpdatePowerItemData();
    }

    private void UpdatePowerItemData()
    {
        if (powerNameText != null)
            powerNameText.text = memory.MemoryName;

        if (powerImage != null)
            powerImage.sprite = memory.MemoryImage;
    }

    public void SetMemory(MemoryScriptableObject memoryObject)
    {
        memory = memoryObject;
    }
}