using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

[RequireComponent(typeof(UniqueId))]
public class GunDisplay : MonoBehaviour, IGunHolder, IInteractable, ILevelLoaderInfo
{
    #region Serialized Fields

    [SerializeField] private GameObject initialGunPrefab;
    [SerializeField] private bool isFree;

    [Space, SerializeField] private Transform gunHolderTransform;
    [SerializeField] private Vector3 popOutAmount;
    [SerializeField, Min(0)] private float positionLerpAmount = 0.1f;

    [Space, SerializeField] private UnityEvent onPurchase;

    #endregion

    #region Private Fields

    private bool _isCurrentlyLookedAt;

    private Vector3 _originalGunScale;

    private bool _hasOriginalGun = true;

    #endregion

    #region Getters

    public IGun EquippedGun { get; private set; }

    public GameObject GameObject => gameObject;

    public bool IsInteractable => EquippedGun != null;

    public bool HasOutline { get; set; }
    public HashSet<Material> OutlineMaterials { get; } = new();

    public InteractionIcon InteractionIcon => InteractionIcon.Pickup;
    
    public UnityEvent OnInteraction => onPurchase;

    #endregion

    public Action<WeaponManager, IGun> OnGunEquipped { get; set; }

    public Action<WeaponManager, IGun> OnGunRemoved { get; set; }

    private void Start()
    {
        if (initialGunPrefab != null && _hasOriginalGun)
        {
            var gun = Instantiate(initialGunPrefab, gunHolderTransform.position, gunHolderTransform.rotation)
                .GetComponent<IGun>();

            // // Get the interactable materials of the gun
            // gun.GetOutlineMaterials(Player.Instance.PlayerInteraction.OutlineMaterial.shader);

            EquipGun(gun);
        }
        else
            _hasOriginalGun = false;
    }

    private void FixedUpdate()
    {
        // Update the gun's position
        UpdateGunPosition();

        // Reset the looked at flag
        _isCurrentlyLookedAt = false;
    }

    private void UpdateGunPosition()
    {
        // If there is no gun equipped, return
        if (EquippedGun == null)
            return;

        // Get the gun's rigidbody
        if (!EquippedGun.GameObject.TryGetComponent(out Rigidbody rb))
            return;

        var cOffset = Vector3.zero;

        // Pop the weapon off the holder if it is being looked at
        if (_isCurrentlyLookedAt)
        {
            // Calculate the pop out amount based on the forward direction of the gun holder
            cOffset = (gunHolderTransform.forward * popOutAmount.z) +
                      (gunHolderTransform.up * popOutAmount.y) +
                      (gunHolderTransform.right * popOutAmount.x);
        }

        // Calculate the new position and rotation
        var newPosition = gunHolderTransform.position + cOffset;
        var newRotation = gunHolderTransform.rotation;

        const float fixedFrameTime = 1 / 50f;
        var frameAmount = Time.fixedDeltaTime / fixedFrameTime;

        // Lerp the position and rotation
        rb.MovePosition(Vector3.Lerp(rb.position, newPosition, positionLerpAmount * frameAmount));
        rb.MoveRotation(Quaternion.Lerp(rb.rotation, newRotation, positionLerpAmount * frameAmount));

        // Set the scale back to the original scale
        EquippedGun.GameObject.transform.localScale = _originalGunScale;
    }

    public void EquipGun(IGun gun)
    {
        // Return if the gun is null
        if (gun == null)
            return;

        // If the gun is already equipped, return
        if (EquippedGun == gun)
            return;

        // If there is already a gun equipped, remove it
        if (EquippedGun != null)
            RemoveGun();

        // Get the gun's current global scale
        _originalGunScale = gun.GameObject.transform.lossyScale;

        // Set the equipped gun
        EquippedGun = gun;

        // Set the gun's parent to the gun holder
        gun.GameObject.transform.SetParent(gunHolderTransform, true);

        // Make the weapon kinematic
        if (gun.GameObject.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = true;

            // Also, disable the collider
            if (gun.Collider != null)
                gun.Collider.enabled = false;

            // Disable collision on the gun
            rb.detectCollisions = false;

            // Set the rigid body to interpolate
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        // Set the gun's scale back to the original scale
        gun.GameObject.transform.localScale = _originalGunScale;

        // Invoke the OnGunEquipped event
        OnGunEquipped?.Invoke(null, gun);
    }

    public void RemoveGun()
    {
        // If there is no gun equipped, return
        if (EquippedGun == null)
            return;

        // Set the has original gun flag to false
        _hasOriginalGun = false;

        // Set the gun's parent to null
        EquippedGun.GameObject.transform.SetParent(null, true);

        // Make the weapon non-kinematic (if it has a rigidbody)
        if (EquippedGun.GameObject.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = false;

            // Also, enable the collisions
            rb.detectCollisions = true;

            // // Throw the gun
            // ThrowRigidBody(rb);

            // Turn off the rigid body's interpolation
            rb.interpolation = RigidbodyInterpolation.None;
        }

        // Invoke the OnGunRemoved event
        OnGunRemoved?.Invoke(null, EquippedGun);

        // Set the gun's scale back to the original scale
        EquippedGun.GameObject.transform.localScale = _originalGunScale;

        // Set the equipped gun to null
        EquippedGun = null;
    }

    public void Interact(PlayerInteraction playerInteraction)
    {
        // If there is no gun equipped, return
        if (EquippedGun == null)
            return;

        var gun = EquippedGun;

        // Get the player's inventory
        var playerInventory = playerInteraction.Player.PlayerInventory;

        // Get the player's money count
        var playerMoney = playerInventory.GetItemCount(playerInventory.MoneyObject);

        // If the gun is not free and the player does not have enough money, return
        if (!isFree)
        {
            if (playerMoney < gun.GunInformation.Cost)
            {
                // Send a tooltip to the player
                JournalTooltipManager.Instance.AddTooltip(
                    new BasicJournalTooltipInfo(
                        $"You need more money to purchase {gun.GunInformation.GunName}.",
                        JournalTooltipType.General
                    )
                );

                return;
            }

            // Remove the money from the player's inventory
            playerInventory.RemoveItem(playerInventory.MoneyObject, gun.GunInformation.Cost);

            // Send a tooltip to the player
            JournalTooltipManager.Instance.AddTooltip(
                new BasicJournalTooltipInfo(
                    $"Purchased {gun.GunInformation.GunName} for ${gun.GunInformation.Cost}!",
                    JournalTooltipType.General
                )
            );

            // Set the holder to be free from now on
            isFree = true;

            // Invoke the onInteract event
            onPurchase?.Invoke();
        }


        // Remove the gun
        RemoveGun();

        var playerGun = playerInteraction.Player.WeaponManager.EquippedGun;

        // If the player's gun is not null, remove it
        if (playerGun != null)
            playerInteraction.Player.WeaponManager.RemoveGun();

        // Equip the gun to the player
        playerInteraction.Player.WeaponManager.EquipGun(gun);

        // Equip the player's old gun to the gun display
        EquipGun(playerGun);
    }

    public void LookAtUpdate(PlayerInteraction playerInteraction)
    {
        _isCurrentlyLookedAt = true;
    }

    public string InteractText(PlayerInteraction playerInteraction)
    {
        if (EquippedGun == null)
            return "";

        if (isFree)
            return $"Pick up {EquippedGun.GunInformation.GunName}";
        else
            return $"Purchase {EquippedGun.GunInformation.GunName} for ${EquippedGun.GunInformation.Cost}";
    }

    #region ILevelLoaderInfo

    private const string IS_FREE_KEY = "_isFree";
    private const string HAS_ORIGINAL_GUN_KEY = "_hasOriginalGun";

    private UniqueId _uniqueId;

    public UniqueId UniqueId
    {
        get
        {
            if (_uniqueId == null)
                _uniqueId = GetComponent<UniqueId>();

            return _uniqueId;
        }
    }

    public void LoadData(LevelLoader levelLoader)
    {
        // Load the isFree data
        if (levelLoader.TryGetDataFromMemory(UniqueId, IS_FREE_KEY, out bool isFreeValue))
            isFree = isFreeValue;

        var hasOriginalGunDataExists = levelLoader.TryGetDataFromMemory(UniqueId, HAS_ORIGINAL_GUN_KEY, out bool hasOriginalGun);

        // set the has original gun flag if the data exists
        if (hasOriginalGunDataExists)
            _hasOriginalGun = hasOriginalGun;
    }

    public void SaveData(LevelLoader levelLoader)
    {
        // Save the isFree data
        var isFreeData = new DataInfo(IS_FREE_KEY, isFree);
        levelLoader.AddDataToMemory(UniqueId, isFreeData);

        // Save the hasGun data
        var hasOriginalGunData = new DataInfo(HAS_ORIGINAL_GUN_KEY, _hasOriginalGun);
        levelLoader.AddDataToMemory(UniqueId, hasOriginalGunData);
    }

    #endregion
}