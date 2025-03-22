using System;
using TMPro;
using UnityEngine;

public class PauseMoneyCounter : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private IntReference moneyCount;
    
    private void Update()
    {
        // Set the text to the money count
        SetText();
    }

    private void SetText()
    {
        text.text = $"${moneyCount.Value}";
    }
}