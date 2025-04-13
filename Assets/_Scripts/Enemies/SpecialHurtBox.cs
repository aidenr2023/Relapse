using UnityEngine;
using UnityEngine.Events;

public class SpecialHurtBox : MonoBehaviour
{
    [SerializeField, Min(0)] private float damageMultiplier = 1.5f;
    [field: SerializeField] public UnityEvent<SpecialHurtBox> OnHit { get; private set; }

    public float DamageMultiplier => damageMultiplier;
}