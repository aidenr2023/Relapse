using UnityEngine;

[CreateAssetMenu(fileName = "GunInformation", menuName = "Gun/Gun Information", order = 0)]
public class GunInformation : ScriptableObject
{
    public enum FireType
    {
        Single,
        Burst,
        Spread,
        Automatic
    }

    [SerializeField] private string gunName;

    [SerializeField] private FireType fireType;
    
    [SerializeField] private float baseDamage;
    [SerializeField] private float fireRate;
}