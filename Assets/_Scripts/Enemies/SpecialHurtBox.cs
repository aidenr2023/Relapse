using UnityEngine;

public class SpecialHurtBox : MonoBehaviour
{
    [SerializeField, Min(0)] private float damageMultiplier = 1.5f;
    
    public float DamageMultiplier => damageMultiplier;
}