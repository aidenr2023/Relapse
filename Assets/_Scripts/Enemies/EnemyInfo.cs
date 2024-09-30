using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Enemy))]
public class EnemyInfo : MonoBehaviour, IActor
{
    #region Fields

    [SerializeField] private float maxHealth = 3f;
    [SerializeField] private float currentHealth;

    #endregion

    #region Getters

    public GameObject GameObject => gameObject;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;

    #endregion

    public void ChangeHealth(float amount)
    {
        // Clamp the health value between 0 and the max health
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);

        // If the enemy's health is less than or equal to 0, call the Die function
        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        // TODO: Implement death logic
        Destroy(gameObject);
    }
}