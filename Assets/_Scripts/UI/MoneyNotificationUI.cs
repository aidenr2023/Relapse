using System;
using TMPro;
using UnityEngine;

public class MoneyNotificationUI : MonoBehaviour
{
    private const float ALPHA_THRESHOLD = 0.001f;

    #region Serialized Fields

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text moneyAddedText;

    [SerializeField] private CanvasGroup totalMoneyCanvasGroup;
    [SerializeField] private TMP_Text totalMoneyText;

    [SerializeField, Min(0)] private float fadeLerpAmount;

    [SerializeField, Min(0)] private float waitBeforeTotalTime = 3f;
    [SerializeField, Min(0)] private float stayOnScreenTime = 2f;

    [SerializeField, Range(0, 1)] private float maxOpacity = 1;

    #endregion

    #region Private Fields

    private int _moneyAmount;

    private float _desiredAlpha;

    private CountdownTimer _waitBeforeTotalTimer;
    private CountdownTimer _stayOnScreenTimer;

    #endregion

    private void Awake()
    {
        // Set the canvas group alpha to 0
        canvasGroup.alpha = 0;

        // Set up the timers
        _waitBeforeTotalTimer = new CountdownTimer(waitBeforeTotalTime);
        _stayOnScreenTimer = new CountdownTimer(stayOnScreenTime);

        _waitBeforeTotalTimer.Start();
    }

    private void Start()
    {
        // Subscribe to the inventory's OnItemAdded event
        Player.Instance.PlayerInventory.OnItemAdded += MoneyNotificationOnPickup;
        Player.Instance.PlayerInventory.OnItemRemoved += MoneyNotificationOnRemoval;
    }

    private void MoneyNotificationOnPickup(InventoryObject item, int quantity)
    {
        // Return if there is no player
        if (Player.Instance == null)
            return;

        // If the item is not the money object, return
        if (item != Player.Instance.PlayerInventory.MoneyObject)
            return;

        var initialMoneyAmount = _moneyAmount;

        // Add the quantity to the money amount
        _moneyAmount += quantity;

        // Set the desired alpha to maxOpacity
        _desiredAlpha = maxOpacity;

        // Reset the stay on screen timer and the wait before total timer
        if (initialMoneyAmount == 0)
            _waitBeforeTotalTimer.SetMaxTimeAndReset(waitBeforeTotalTime);

        _stayOnScreenTimer.SetMaxTimeAndReset(stayOnScreenTime);
    }

    private void MoneyNotificationOnRemoval(InventoryObject item, int quantity)
    {
        MoneyNotificationOnPickup(item, -quantity);
    }

    private void Update()
    {
        // Update the timers
        _waitBeforeTotalTimer.SetMaxTime(waitBeforeTotalTime);
        _waitBeforeTotalTimer.Update(Time.unscaledDeltaTime);

        _stayOnScreenTimer.SetMaxTime(stayOnScreenTime);
        _stayOnScreenTimer.Update(Time.unscaledDeltaTime);

        // Determine if the stay on screen timer should be active based on the wait before total timer
        _stayOnScreenTimer.SetActive(_waitBeforeTotalTimer.IsComplete);

        const float defaultFrameTime = 1 / 60f;
        var frameAmount = Time.unscaledDeltaTime / defaultFrameTime;

        // If the stay on screen timer is complete, set the desired alpha to 0
        if (_stayOnScreenTimer.IsComplete)
            _desiredAlpha = 0;

        // Lerp the canvas group's alpha to the desired alpha
        canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, _desiredAlpha, fadeLerpAmount * frameAmount);

        if (Mathf.Abs(canvasGroup.alpha - _desiredAlpha) < ALPHA_THRESHOLD)
            canvasGroup.alpha = _desiredAlpha;

        // Calculate the desired alpha for the total money canvas group
        var isTotalActive = _waitBeforeTotalTimer.IsComplete;
        var desiredTotalMoneyAlpha = isTotalActive ? 1 : 0;

        totalMoneyCanvasGroup.alpha = Mathf.Lerp(
            totalMoneyCanvasGroup.alpha, desiredTotalMoneyAlpha,
            fadeLerpAmount * frameAmount
        );

        if (Mathf.Abs(totalMoneyCanvasGroup.alpha - desiredTotalMoneyAlpha) < ALPHA_THRESHOLD)
            totalMoneyCanvasGroup.alpha = desiredTotalMoneyAlpha;

        // If the total money canvas group's alpha is 0, set the total money canvas group's alpha to 0
        // Also, set the money amount to 0
        if (canvasGroup.alpha == 0)
        {
            totalMoneyCanvasGroup.alpha = 0;
            _moneyAmount = 0;
        }

        // Update the text
        UpdateText();
    }

    private void UpdateText()
    {
        var icon = (_moneyAmount >= 0) ? '+' : '-';

        // Set the money added text
        moneyAddedText.text = $"{icon} ${Mathf.Abs(_moneyAmount)}";

        // Get the inventory entry for the money object
        var totalMoneyCount = Player.Instance.PlayerInventory.GetItemCount(Player.Instance.PlayerInventory.MoneyObject);

        // Set the total money text
        totalMoneyText.text = $"Total: ${totalMoneyCount}";
    }
}