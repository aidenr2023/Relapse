using System;
using UnityEngine;

[Serializable]
public struct PlayerSerializationData
{
    [SerializeField] public Vector3 position;
    [SerializeField] public Quaternion rotation;
    [SerializeField] public Vector3 velocity;

    [Space, SerializeField] public float currentHealth;
    [SerializeField] public float maxHealth;
    [SerializeField] public float currentToxicity;
    [SerializeField] public float maxToxicity;
    [SerializeField] public int relapseCount;
    [SerializeField] public bool isRelapsing;

    [Space, SerializeField] public int currentPowerIndex;
    [SerializeField] public PowerScriptableObject[] equippedPowers;

    [Space, SerializeField] public GameObject equippedGun;
    [SerializeField] public int currentAmmo;

    [Space, SerializeField] public InventoryEntry[] inventoryEntries;

    [Space, SerializeField] public float currentStamina;
    [SerializeField] public float maxStamina;
    [SerializeField] public bool canSprint;
    [SerializeField] public bool canJump;
    [SerializeField] public bool canWallRun;
    [SerializeField] public bool canSlide;
}