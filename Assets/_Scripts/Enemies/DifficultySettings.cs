using UnityEngine;

[CreateAssetMenu(fileName = "DifficultySettings", menuName = "Enemies/DifficultySettings")]
public class DifficultySettings : ScriptableObject
{
    [SerializeField] private float difficultyHealthMultiplier = 1;
    [SerializeField] private float difficultyDamageMultiplier = 1;
    [SerializeField] private float difficultySpeedMultiplier = 1;
    
    public float DifficultyHealthMultiplier => difficultyHealthMultiplier;
    public float DifficultyDamageMultiplier => difficultyDamageMultiplier;
    public float DifficultySpeedMultiplier => difficultySpeedMultiplier;
}