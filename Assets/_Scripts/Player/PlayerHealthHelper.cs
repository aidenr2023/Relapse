using UnityEngine;

public class PlayerHealthHelper : MonoBehaviour, IDamager
{
    public GameObject GameObject => gameObject;
    public Sound NormalHitSfx => null;
    public Sound CriticalHitSfx => null;

    public void SetToHealthPercent(float percent)
    {
        // Clamp the percent between 0 and 1
        percent = Mathf.Clamp01(percent);

        var playerInfo = Player.Instance.PlayerInfo;

        var maxHealth = playerInfo.MaxHealth;
        var currentHealth = playerInfo.CurrentHealth;

        var percentageHealth = maxHealth * percent;

        // Get the difference between the percentage health and the current health
        var difference = percentageHealth - currentHealth;

        // Change the health based on the difference
        playerInfo.ChangeHealth(difference, playerInfo, this, playerInfo.transform.position, false);
    }

    public void SetToFullHealth() => SetToHealthPercent(1);

    public void KillPlayer() => SetToHealthPercent(0);
}